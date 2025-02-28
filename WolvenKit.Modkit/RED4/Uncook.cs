using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WolvenKit.Common;
using WolvenKit.Common.DDS;
using WolvenKit.Common.Extensions;
using WolvenKit.Common.Model.Arguments;
using WolvenKit.Common.Services;
using WolvenKit.Modkit.Extensions;
using WolvenKit.Modkit.RED4.Opus;
using WolvenKit.RED4.CR2W;
using WolvenKit.RED4.CR2W.Archive;
using WolvenKit.RED4.Types;

namespace WolvenKit.Modkit.RED4
{
    public partial class ModTools
    {
        /// <summary>
        /// Uncooks a single file by hash. This will both extract and uncook the redengine file
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="hash"></param>
        /// <param name="outDir"></param>
        /// <param name="rawOutDir"></param>
        /// <param name="args"></param>
        /// <param name="forcebuffers"></param>
        /// <returns></returns>
        public bool UncookSingle(
            Archive archive,
            ulong hash,
            DirectoryInfo outDir,
            GlobalExportArgs args,
            DirectoryInfo rawOutDir = null,
            ECookedFileFormat[] forcebuffers = null)
        {
            if (!archive.Files.ContainsKey(hash))
            {
                return false;
            }

            #region unbundle main file

            using var cr2WStream = new MemoryStream();
            ExtractSingleToStream(archive, hash, cr2WStream);

            if (archive.Files[hash] is FileEntry entry)
            {
                var relFileFullName = entry.FileName;
                if (string.IsNullOrEmpty(Path.GetExtension(relFileFullName)))
                {
                    relFileFullName += ".bin";
                }

                var mainFileInfo = new FileInfo(Path.Combine(outDir.FullName, $"{relFileFullName}"));

                // write mainFile
                if (!WolvenTesting.IsTesting)
                {
                    if (mainFileInfo.Directory != null)
                    {
                        Directory.CreateDirectory(mainFileInfo.Directory.FullName);
                    }

                    using var fs = new FileStream(mainFileInfo.FullName, FileMode.Create, FileAccess.Write);
                    cr2WStream.Seek(0, SeekOrigin.Begin);
                    cr2WStream.CopyTo(fs);
                }

                #endregion unbundle main file

                #region extract buffers

                var hasBuffers = (entry.SegmentsEnd - entry.SegmentsStart) > 1;
                if (!hasBuffers)
                {
                    return true;
                }

                // uncook main file buffers to raw out dir
                if (rawOutDir is null or { Exists: false })
                {
                    rawOutDir = outDir;
                }

                try
                {
                    // wems need the physical infile path
                    args.Get<WemExportArgs>().FileName = mainFileInfo.FullName;
                    return UncookBuffers(cr2WStream, relFileFullName, args, rawOutDir, forcebuffers);
                }
                catch (Exception e)
                {
                    _loggerService.Error($"{relFileFullName} And unexpected error occured while uncooking: {e.Message}");
                    _loggerService.Error(e);
                    return false;
                }
            }

            return false;

            #endregion extract buffers
        }

        /// <summary>
        /// Uncooks all Files to the specified directory.
        /// </summary>
        /// <param name="ar"></param>
        /// <param name="outDir"></param>
        /// <param name="args"></param>
        /// <param name="unbundle"></param>
        /// <param name="pattern"></param>
        /// <param name="regex"></param>
        /// <param name="rawOutDir"></param>
        /// <param name="forcebuffers"></param>
        /// <returns></returns>
        public void UncookAll(
            Archive ar,
            DirectoryInfo outDir,
            GlobalExportArgs args,
            bool unbundle = false,
            string pattern = "",
            string regex = "",
            DirectoryInfo rawOutDir = null,
            ECookedFileFormat[] forcebuffers = null)
        {
            var extractedList = new ConcurrentBag<string>();
            var failedList = new ConcurrentBag<string>();

            // check search pattern then regex
            var finalmatches = ar.Files.Values.Cast<FileEntry>();
            var totalInArchiveCount = ar.FileCount;
            if (!string.IsNullOrEmpty(pattern))
            {
                finalmatches = ar.Files.Values.Cast<FileEntry>().MatchesWildcard(item => item.FileName, pattern);
            }

            if (!string.IsNullOrEmpty(regex))
            {
                var searchTerm = new System.Text.RegularExpressions.Regex($@"{regex}");
                var queryMatchingFiles =
                    from file in finalmatches
                    let matches = searchTerm.Matches(file.FileName)
                    where matches.Count > 0
                    select file;

                finalmatches = queryMatchingFiles;
            }

            var finalMatchesList = finalmatches.ToList();
            if (!unbundle)
            {
                finalMatchesList = finalMatchesList.Where(_ => ar.CanUncook(_.NameHash64)).ToList();
            }

            _loggerService.Info($" {ar.ArchiveAbsolutePath}: Found {finalMatchesList.Count}/{totalInArchiveCount} entries to uncook");
            if (finalMatchesList.Count == 0)
            {
                return;
            }

            var progress = 0;
            _progressService.Report(0);

            // check hashes before parallel unbundling
            for (var i = 0; i < finalMatchesList.Count; i++)
            {
                var item = finalMatchesList[i];
                _hashService.Contains(item.Key);
            }

            //foreach (var info in finalMatchesList)
            Parallel.ForEach(finalMatchesList, info =>
            {
                if (UncookSingle(ar, info.NameHash64, outDir, args, rawOutDir, forcebuffers))
                {
                    extractedList.Add(info.FileName);
                }
                else
                {
                    failedList.Add(info.FileName);
                }

                Interlocked.Increment(ref progress);
                _progressService.Report(progress / (float)finalMatchesList.Count);
            }
            );

            foreach (var failed in failedList)
            {
                _loggerService.Warning($"Failed to uncook {failed}.");
            }

            _loggerService.Success($" {ar.ArchiveAbsolutePath}: Uncooked {extractedList.Count}/{finalMatchesList.Count} files.");
        }

        /// <summary>
        /// Extracts and decompresses buffers of a cr2wstream
        /// uncooks buffers to raw format
        /// </summary>
        /// <param name="cr2wStream">the cooked redengine file input stream</param>
        /// <param name="relPath">if a depot is used the relpath is a relative base path, if no depot is used, the relapth is simply the filename</param>
        /// <param name="settings">GlobalExportSettings</param>
        /// <param name="rawOutDir">the output directory. the outfile is conbined from the rawoutdir and the relative path</param>
        /// <param name="forcebuffers"></param>
        /// <returns></returns>
        private bool UncookBuffers(Stream cr2wStream, string relPath, GlobalExportArgs settings, DirectoryInfo rawOutDir, ECookedFileFormat[] forcebuffers = null)
        {
            var outfile = new FileInfo(Path.Combine(rawOutDir.FullName, relPath));
            if (outfile.Directory == null)
            {
                return false;
            }

            var ext = Path.GetExtension(relPath).TrimStart('.');

            // files where uncook methods are NOT implemented: just extract buffers
            if (!WolvenTesting.IsTesting)
            {
                Directory.CreateDirectory(outfile.Directory.FullName);
            }

            var isForcedBuffer = forcebuffers != null && forcebuffers.Any() && forcebuffers.Select(_ => _.ToString()).Contains(ext);
            if (!Enum.GetNames(typeof(ECookedFileFormat)).Contains(ext) || isForcedBuffer)
            {
                var i = 0;
                foreach (var stream in GenerateBuffers(cr2wStream))
                {
                    var bufferpath = $"{outfile.FullName}.{i}.buffer";

                    if (!WolvenTesting.IsTesting)
                    {
                        using var fs = new FileStream(bufferpath, FileMode.Create, FileAccess.Write);
                        i++;
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fs);
                    }

                    stream.Dispose();
                }

                return true;
            }

            // handle files where uncook methods ARE implemented
            if (!Enum.TryParse(ext, true, out ECookedFileFormat extAsEnum))
            {
                return false;
            }

            // wems are not cr2w files, need to be handled first
            if (extAsEnum == ECookedFileFormat.wem)
            {
                if (settings.Get<WemExportArgs>() is { } wemaArgs)
                {
                    var wemoutfile = Path.ChangeExtension(outfile.FullName, wemaArgs.wemExportType.ToString());
                    UncookWem(wemaArgs.FileName, wemoutfile);
                    return true;
                }

                return false;
            }

            // uncook textures, meshes etc
            //args.FileName = outFileInfo.FullName;
            switch (extAsEnum)
            {
                //case ECookedFileFormat.ent:
                //case ECookedFileFormat.app:
                //    return HandleEntity(cr2wStream, outfile, settings.Get<EntityExportArgs>());
                case ECookedFileFormat.opusinfo:
                    return HandleOpus(settings.Get<OpusExportArgs>());

                case ECookedFileFormat.mlmask:
                    return UncookMlmask(cr2wStream, outfile, settings.Get<MlmaskExportArgs>());

                case ECookedFileFormat.mesh:
                    try
                    {
                        return HandleMesh(cr2wStream, outfile, settings.Get<MeshExportArgs>());
                    }
                    catch (Exception e)
                    {
                        _loggerService.Error($"{relPath} - {e.Message}");
                        _loggerService.Error(e);

                        return false;
                    }

                case ECookedFileFormat.morphtarget:
                    // WolvenKit.CLI does not use (i.e. set) the App's settings configuration, resulting in a null value for
                    // settings.Get<MorphTargetExportArgs>().ModFolderPath.  In the context of the CLI, output directory
                    // (i.e. -o argument) can be used to provide the correct absolute pathname.
                    var modFolderPath = settings.Get<MorphTargetExportArgs>().ModFolderPath;
                    if (modFolderPath == null)
                    {
                        modFolderPath = rawOutDir.FullName;
                    }
                    return ExportMorphTargets(cr2wStream, outfile, settings.Get<MorphTargetExportArgs>().Archives, modFolderPath, settings.Get<MorphTargetExportArgs>().IsBinary);
                case ECookedFileFormat.anims:
                    try
                    {
                        return ExportAnim(cr2wStream, settings.Get<AnimationExportArgs>().Archives, outfile, settings.Get<AnimationExportArgs>().IsBinary);
                    }
                    catch (Exception e)
                    {
                        _loggerService.Error($"{relPath} - {e.Message}");
                        _loggerService.Error(e);

                        return false;
                    }
                case ECookedFileFormat.xbm:
                {
                    if (settings.Get<XbmExportArgs>() is not { } xbmargs)
                    {
                        return false;
                    }

                    DXGI_FORMAT texformat;

                    if (WolvenTesting.IsTesting)
                    {
                        using var ms = new MemoryStream();
                        return ConvertXbmToDdsStream(cr2wStream, ms, out texformat);
                    }

                    using (var ms = new MemoryStream())
                    {
                        if (!ConvertXbmToDdsStream(cr2wStream, ms, out texformat))
                        {
                            return false;
                        }

                        // convert if needed else save to file
                        var ddsPath = Path.ChangeExtension(outfile.FullName, ERawFileFormat.dds.ToString());
                        if (xbmargs.UncookExtension != EUncookExtension.dds)
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            return Texconv.ConvertFromDdsAndSave(ms, ddsPath, xbmargs);
                        }
                        else
                        {
                            //TODO: flip dds

                            using var fs = new FileStream(ddsPath, FileMode.Create, FileAccess.Write);
                            ms.Seek(0, SeekOrigin.Begin);
                            ms.CopyTo(fs);
                        }
                    }

                    return true;
                }
                case ECookedFileFormat.csv:
                {
                    if (WolvenTesting.IsTesting)
                    {
                        using var ms = new MemoryStream();
                        return UncookCsv(cr2wStream, ms);
                    }

                    using var fs = new FileStream($"{outfile.FullName}.csv", FileMode.Create, FileAccess.Write);
                    return UncookCsv(cr2wStream, fs);
                }
                //case ECookedFileFormat.json:
                //    return true;
                case ECookedFileFormat.cubemap:
                {
                    if (WolvenTesting.IsTesting)
                    {
                        using var ms = new MemoryStream();
                        return UncookCubeMap(cr2wStream, ms);
                    }
                    var newpath = Path.ChangeExtension(outfile.FullName, ERawFileFormat.dds.ToString());
                    using var ddsStream = new FileStream($"{newpath}", FileMode.Create, FileAccess.Write);
                    return UncookCubeMap(cr2wStream, ddsStream);
                }
                case ECookedFileFormat.envprobe:
                {
                    if (WolvenTesting.IsTesting)
                    {
                        using var ms = new MemoryStream();
                        return UncookEnvprobe(cr2wStream, ms);
                    }
                    var newpath = Path.ChangeExtension(outfile.FullName, ERawFileFormat.dds.ToString());
                    using var ddsStream = new FileStream($"{newpath}", FileMode.Create, FileAccess.Write);
                    return UncookEnvprobe(cr2wStream, ddsStream);
                }
                case ECookedFileFormat.texarray:
                {
                    if (WolvenTesting.IsTesting)
                    {
                        using var ms = new MemoryStream();
                        return UncookTexarray(cr2wStream, ms);
                    }
                    var newpath = Path.ChangeExtension(outfile.FullName, ERawFileFormat.dds.ToString());
                    using var ddsStream = new FileStream($"{newpath}", FileMode.Create, FileAccess.Write);
                    return UncookTexarray(cr2wStream, ddsStream);
                }
                case ECookedFileFormat.fnt:
                {
                    if (WolvenTesting.IsTesting)
                    {
                        using var ms = new MemoryStream();
                        return UncookFont(cr2wStream, ms);
                    }
                    var newpath = Path.ChangeExtension(outfile.FullName, ERawFileFormat.ttf.ToString());
                    using var ttfStream = new FileStream($"{newpath}", FileMode.Create, FileAccess.Write);
                    return UncookFont(cr2wStream, ttfStream);
                }
                default:
                    throw new ArgumentOutOfRangeException($"Uncooking failed for extension: {extAsEnum}.");
            }
        }

        private bool HandleEntity(Stream cr2wStream, FileInfo cr2wFileName, EntityExportArgs entExportArgs)
        {
            try
            {
                switch (entExportArgs.ExportType)
                {
                    case EntityExportType.Json:
                        return DumpEntityPackageAsJson(cr2wStream, cr2wFileName);
                    case EntityExportType.Gltf:
                        throw new NotImplementedException("Uncooking Entity/Appearance Resources To Gltf Not Implemented");
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                _loggerService.Error(ex);
            }
            return false;
        }

        private static bool HandleOpus(OpusExportArgs opusExportArgs)
        {
            OpusTools opusTools = new(
                opusExportArgs.SoundbanksArchive,
                opusExportArgs.ModFolderPath,
                opusExportArgs.RawFolderPath,
                opusExportArgs.UseMod);

            // If More than 0 selected from opusinfo export to wem.
            if (opusExportArgs.SelectedForExport.Count > 0)
            {
                foreach (var audiofile in opusExportArgs.SelectedForExport)
                {
                    opusTools.ExportOpusUsingHash(audiofile);
                }
            }

            return true;
        }

        private bool UncookFont(Stream redstream, Stream outstream)
        {
            if (!_wolvenkitFileService.TryReadRed4File(redstream, out var cr2w))
            {
                return false;
            }

            if (cr2w.RootChunk is not rendFont font)
            {
                return false;
            }

            outstream.Write(font.FontBuffer.Buffer.GetBytes());

            return true;
        }

        private bool HandleMesh(Stream cr2wStream, FileInfo cr2wFileName, MeshExportArgs meshargs)
        {
            switch (meshargs.meshExportType)
            {
                case MeshExportType.Default:
                    return _meshTools.ExportMesh(cr2wStream, cr2wFileName, meshargs.LodFilter, meshargs.isGLBinary);

                case MeshExportType.WithMaterials:
                    return ExportMeshWithMaterials(cr2wStream, cr2wFileName, meshargs.Archives, meshargs.MaterialRepo,
                        meshargs.MaterialUncookExtension, meshargs.isGLBinary, meshargs.LodFilter);

                case MeshExportType.WithRig:
                {
                    var entry = meshargs.Rig.FirstOrDefault();
                    if (entry == null)
                    {
                        return false;
                    }

                    var ar = entry.Archive as Archive;
                    using var ms = new MemoryStream();
                    ar?.CopyFileToStream(ms, entry.NameHash64, false);

                    return _meshTools.ExportMeshWithRig(cr2wStream, ms, cr2wFileName, meshargs.LodFilter, meshargs.isGLBinary);
                }
                case MeshExportType.Multimesh:
                {
                    var meshes = meshargs.MultiMeshMeshes;
                    var rigs = meshargs.MultiMeshRigs;
                    if (!meshes.Any() || !rigs.Any())
                    {
                        return false;
                    }

                    var meshstreams = meshes.Select(
                            delegate (FileEntry entry)
                            {
                                var ar = entry.Archive as Archive;
                                var ms = new MemoryStream();
                                ar?.CopyFileToStream(ms, entry.NameHash64, false);
                                return (Stream)ms;
                            })
                        .ToList();

                    var rigstreams = rigs.Select(
                            delegate (FileEntry entry)
                            {
                                var ar = entry.Archive as Archive;
                                var ms = new MemoryStream();
                                ar?.CopyFileToStream(ms, entry.NameHash64, false);
                                return (Stream)ms;
                            })
                        .ToList();

                    return _meshTools.ExportMultiMeshWithRig(meshstreams, rigstreams, cr2wFileName, meshargs.LodFilter, meshargs.isGLBinary);
                }
                default:
                    return false;
            }
        }

        private static void UncookWem(string infile, string outfile)
        {
            if (WolvenTesting.IsTesting)
            {
                return;
            }

            var arg = infile.ToEscapedPath() + " -o " + outfile.ToEscapedPath();
            var si = new ProcessStartInfo(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "vgmstream", "test.exe"),
                    arg
                )
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Verb = "runas"
            };
            var proc = Process.Start(si);
            if (proc != null)
            {
                proc.WaitForExit();
                Trace.WriteLine(proc.StandardOutput.ReadToEnd());
            }
        }

        public IEnumerable<Stream> GenerateBuffers(Stream cr2wStream)
        {
            // read the cr2wfile
            if (!_wolvenkitFileService.TryReadRed4File(cr2wStream, out var cr2w))
            {
                yield break;
            }

            foreach (var buffer in cr2w.GetBuffers())
            {
                var ms = new MemoryStream();
                ms.Write(buffer.GetBytes());

                yield return ms;
            }
        }

        private bool UncookTexarray(Stream cr2wStream, Stream outstream)
        {
            // read the cr2wfile
            var cr2w = _wolvenkitFileService.ReadRed4File(cr2wStream);
            if (cr2w == null || cr2w.RootChunk is not CTextureArray texa || texa.RenderTextureResource.RenderResourceBlobPC.Chunk is not rendRenderTextureBlobPC blob)
            {
                return false;
            }

            var sliceCount = blob.Header.TextureInfo.SliceCount;
            var mipCount = blob.Header.TextureInfo.MipCount;
            var alignment = blob.Header.TextureInfo.DataAlignment;

            var height = blob.Header.SizeInfo.Height;
            var width = blob.Header.SizeInfo.Width;

            var rawfmt = Enums.ETextureRawFormat.TRF_Invalid;
            if (texa.Setup.RawFormat?.Value != null)
            {
                rawfmt = texa.Setup.RawFormat.Value.Value;
            }

            var compression = Enums.ETextureCompression.TCM_None;
            if (texa.Setup.Compression?.Value != null)
            {
                compression = texa.Setup.Compression.Value.Value;
            }

            var texformat = CommonFunctions.GetDXGIFormat(compression, rawfmt, _loggerService);

            DDSUtils.GenerateAndWriteHeader(outstream,
                new DDSMetadata(width, height, 1, sliceCount, mipCount,
                    0, 0, texformat, TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D, alignment, true));

            outstream.Write(blob.TextureData.Buffer.GetBytes());

            return true;
        }

        private bool UncookEnvprobe(Stream cr2wStream, Stream outstream)
        {
            // read the cr2wfile
            var cr2w = _wolvenkitFileService.ReadRed4File(cr2wStream);
            if (cr2w == null || cr2w.RootChunk is not CReflectionProbeDataResource refl || refl.TextureData.RenderResourceBlobPC.Chunk is not rendRenderTextureBlobPC blob)
            {
                return false;
            }

            var sliceCount = blob.Header.TextureInfo.SliceCount;
            var mipCount = blob.Header.TextureInfo.MipCount;
            var alignment = blob.Header.TextureInfo.DataAlignment;

            var height = blob.Header.SizeInfo.Height;
            var width = blob.Header.SizeInfo.Width;

            const DXGI_FORMAT texformat = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;

            DDSUtils.GenerateAndWriteHeader(outstream,
                new DDSMetadata(width, height, 1, sliceCount, mipCount,
                    0, 0, texformat, TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D, alignment, true));

            outstream.Write(blob.TextureData.Buffer.GetBytes());

            return true;
        }

        private bool UncookCubeMap(Stream cr2wStream, Stream outstream)
        {
            // read the cr2wfile
            var cr2w = _wolvenkitFileService.ReadRed4File(cr2wStream);
            if (cr2w == null || cr2w.RootChunk is not CCubeTexture ctex || ctex.RenderTextureResource.RenderResourceBlobPC.Chunk is not rendRenderTextureBlobPC blob)
            {
                return false;
            }

            var sliceCount = blob.Header.TextureInfo.SliceCount;
            var mipCount = blob.Header.TextureInfo.MipCount;
            var alignment = blob.Header.TextureInfo.DataAlignment;

            var height = blob.Header.SizeInfo.Height;
            var width = blob.Header.SizeInfo.Width;

            var compression = Enums.ETextureCompression.TCM_None;
            var rawfmt = Enums.ETextureRawFormat.TRF_Invalid;

            if (ctex.Setup.RawFormat?.Value != null)
            {
                rawfmt = ctex.Setup.RawFormat.Value.Value;
            }

            if (ctex.Setup.Compression?.Value != null)
            {
                compression = ctex.Setup.Compression.Value.Value;
            }

            var texformat = CommonFunctions.GetDXGIFormat(compression, rawfmt, _loggerService);

            DDSUtils.GenerateAndWriteHeader(outstream,
                new DDSMetadata(width, height, 1, sliceCount, mipCount,
                    0, 0, texformat, TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D, alignment, true));

            outstream.Write(blob.TextureData.Buffer.GetBytes());

            return true;
        }

        private bool UncookCsv(Stream cr2wStream, Stream outstream)
        {
            // read the cr2wfile
            if (!_wolvenkitFileService.TryReadRed4File(cr2wStream, out var cr2w))
            {
                return false;
            }

            if (cr2w.RootChunk is not C2dArray redcsv)
            {
                return false;
            }


            redcsv.ToCsvStream(outstream);
            return true;
        }

        public bool ConvertXbmToDdsStream(Stream redInFile, Stream outstream, out DXGI_FORMAT texformat)
        {
            texformat = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;

            // read the cr2wfile
            if (!_wolvenkitFileService.TryReadRed4File(redInFile, out var cr2w))
            {
                return false;
            }

            return ConvertRedClassToDdsStream(cr2w.RootChunk, outstream, out texformat);
        }

        public static bool ConvertRedClassToDdsStream(RedBaseClass cls, Stream outstream, out DXGI_FORMAT texformat)
        {
            texformat = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;

            rendRenderTextureBlobPC blob = null;
            var rawfmt = Enums.ETextureRawFormat.TRF_Invalid;
            var compression = Enums.ETextureCompression.TCM_None;

            if (cls is CBitmapTexture xbm)
            {
                if (xbm.RenderTextureResource.RenderResourceBlobPC != null &&
                    xbm.RenderTextureResource.RenderResourceBlobPC.GetValue() is rendRenderTextureBlobPC xbmBlob)
                {
                    blob = xbmBlob;
                }
                if (xbm.Setup.RawFormat?.Value != null)
                {
                    rawfmt = xbm.Setup.RawFormat.Value.Value;
                }
                if (xbm.Setup.Compression?.Value != null)
                {
                    compression = xbm.Setup.Compression.Value.Value;
                }
            }

            if (cls is CMesh mesh)
            {
                if (mesh.RenderResourceBlob.GetValue() is rendRenderTextureBlobPC meshBlob)
                {
                    blob = meshBlob;
                }
            }

            if (cls is CReflectionProbeDataResource probe)
            {
                if (probe.TextureData.RenderResourceBlobPC.GetValue() is rendRenderTextureBlobPC probeBlob)
                {
                    blob = probeBlob;
                }
            }

            if (blob == null)
            {
                return false;
            }

            #region get xbm data

            var width = blob.Header.SizeInfo.Width;
            var height = blob.Header.SizeInfo.Height;
            var mipCount = blob.Header.TextureInfo.MipCount;
            var sliceCount = blob.Header.TextureInfo.SliceCount;
            var alignment = blob.Header.TextureInfo.DataAlignment;

            texformat = CommonFunctions.GetDXGIFormat(compression, rawfmt, null);

            #endregion get xbm data

            // extract and write dds to stream
            DDSUtils.GenerateAndWriteHeader(outstream,
                new DDSMetadata(width, height, 1, sliceCount, mipCount,
                    0, 0, texformat, TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D, alignment, true));

            outstream.Write(blob.TextureData.Buffer.GetBytes());

            return true;
        }
    }
}
