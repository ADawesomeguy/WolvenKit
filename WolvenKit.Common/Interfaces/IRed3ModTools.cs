using System.IO;
using WolvenKit.Common.Model;
using WolvenKit.Common.Model.Arguments;

namespace WolvenKit.Common.Interfaces
{
    public interface IRed3ModTools
    {
        public bool Import(RedRelativePath rawRelative, GlobalImportArgs args, DirectoryInfo outDir = null);
        //bool ImportFolder(DirectoryInfo inDir, GlobalImportArgs args, DirectoryInfo outDir = null);
    }

}
