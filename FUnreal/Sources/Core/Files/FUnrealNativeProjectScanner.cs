using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FUnreal
{
    public class FUnrealNativeProjectScanner
    {
        private string _rootPath;
        public FUnrealNativeProjectScanner(string rootPath) 
        { 
            _rootPath = rootPath;
        }

        public async Task<List<string>> RetrieveUProjectFilePathsAsync()
        {
            var result = new List<string>();

            var uprojectDirsFiles = await XFilesystem.FindFilesEnumAsync(_rootPath, false, "*.uprojectdirs");

            //Get unique paths from all .uprojectdirs
            ISet<string> uniquePaths = new HashSet<string>();
            foreach (var filePath in uprojectDirsFiles)
            {
                var file = new FUnrealUProjectDirsFile(filePath);
                uniquePaths.UnionWith(file.Paths);
            }

            //Exclude paths within Engine/
            var acceptedRelLookupPath = uniquePaths.Where(path => !path.StartsWith("Engine/"));
            if (!acceptedRelLookupPath.Any())
            {
                XDebug.Erro("Cannot get valid paths from uprojectdirs files: {0}", uprojectDirsFiles.ToArray());
                return result;
            }

            //For each path scan at level +1 to look for .uproject file
            foreach (var relPath in acceptedRelLookupPath)
            {
                var absPath = XFilesystem.PathCombine(_rootPath, relPath);
                var subDirPaths = await XFilesystem.FindDirsEnumAsync(absPath);
                foreach (var subDirPath in subDirPaths)
                {
                    var uprojectFilePath = XFilesystem.FindFile(subDirPath, false, "*.uproject");
                    if (uprojectFilePath == null) continue;

                    result.Add(uprojectFilePath);
                }
            }

            return result;
        }
    }
}
