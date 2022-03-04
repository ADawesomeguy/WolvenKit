using System;
using System.IO;
using System.Threading.Tasks;
using WolvenKit.Common;
using WolvenKit.Common.Extensions;
using WolvenKit.Common.Model;
using WolvenKit.Common.Model.Arguments;

namespace TW3Tools.Tasks
{
    public partial class ConsoleFunctions
    {
        public void ImportTask(string[] path,
            string outDir,
            bool keep
        )
        {
            if (path == null || path.Length < 1)
            {
                _loggerService.Warning("Please fill in an input path.");
                return;
            }

            Parallel.ForEach(path, p =>
            {
                ImportTaskInner(p, outDir, keep);
            });
        }

        private void ImportTaskInner(string path,
            string outDir,
            bool keep
        )
        {
            #region checks

            if (string.IsNullOrEmpty(path))
            {
                _loggerService.Warning("Please fill in an input path.");
                return;
            }

            if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            {
                _loggerService.Warning("Please fill in a valid outdirectory path.");
                return;
            }

            var rawFile = new FileInfo(path);
            var inputDirInfo = new DirectoryInfo(path);

            if (!rawFile.Exists && !inputDirInfo.Exists)
            {
                _loggerService.Warning("Input path does not exist.");
                return;
            }

            var isDirectory = !rawFile.Exists;
            var basedir = rawFile.Exists
                ? new FileInfo(path).Directory
                : inputDirInfo;

            #endregion checks
        }
    }
}
