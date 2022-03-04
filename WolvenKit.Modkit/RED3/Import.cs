using System;
using System.IO;
using WolvenKit.Common;
using WolvenKit.Common.DDS;
using WolvenKit.Common.Extensions;
using WolvenKit.Common.Model;
using WolvenKit.Common.Model.Arguments;
using WolvenKit.Core.Extensions;

namespace WolvenKit.Modkit.RED3
{
    public partial class Red3ModTools
    {
        public bool Import(RedRelativePath rawRelative, GlobalImportArgs args, DirectoryInfo outDir = null)
        {
            #region checks

            if (rawRelative is null or { Exists: false })
            {
                return false;
            }
            if (outDir is not { Exists: true })
            {
                outDir = rawRelative.BaseDirectory;
            }

            #endregion

            //var rawRelative = new RedRelativePath(inDir, rawFile.GetRelativePath(inDir));

            // check if the file can be directly imported
            // if not, rebuild buffers
            Enum.TryParse(rawRelative.Extension, true, out ERawFileFormat extAsEnum);


            // import files
            switch (extAsEnum)
            {
                case ERawFileFormat.bmp:
                case ERawFileFormat.jpg:
                case ERawFileFormat.png:
                case ERawFileFormat.tiff:
                case ERawFileFormat.tga:
                case ERawFileFormat.dds:
                    //return HandleTextures(rawRelative, outDir, args);
                case ERawFileFormat.fbx:
                case ERawFileFormat.gltf:
                case ERawFileFormat.glb:
                    //return ImportGltf(rawRelative, outDir, args.Get<GltfImportArgs>());
                case ERawFileFormat.masklist:
                    //return ImportMlmask(rawRelative, outDir);
                case ERawFileFormat.ttf:
                    //return ImportTtf(rawRelative, outDir, args.Get<CommonImportArgs>());
                case ERawFileFormat.wav:
                    //return ImportWav(rawRelative, outDir, args.Get<OpusImportArgs>());
                case ERawFileFormat.csv:
                    //return ImportCsv(rawRelative, outDir, args);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
