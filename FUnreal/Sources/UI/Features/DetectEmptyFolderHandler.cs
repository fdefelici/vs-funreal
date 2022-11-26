using System.Diagnostics;
using System.Threading.Tasks;

namespace FUnreal
{
    internal class DetectEmptyFolderHandler
    {
        private FUnrealVS unrealVS;
        private FUnrealService unrealService;

        public DetectEmptyFolderHandler(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            this.unrealVS = unrealVS;
            this.unrealService = unrealService;
        }

        public async Task ExecuteAsync()
        {
            unrealVS.Output.Warn("DetectEmptyHandler - Start");
            
            Stopwatch watch = Stopwatch.StartNew();

            XDebug.Info("Discovering empty folders ...");

            /*
            var project = unrealService.GetUProject();
            var emptyFolders = await XFilesystem.FindEmptyFoldersAsync(project.PluginsPath, project.SourcePath);

            XDebug.Info($"Discovered empty folders count: {emptyFolders.Count}");

            unrealService.TrackEmptyFolders(emptyFolders);

            foreach (var emptyFolder in emptyFolders)
            {
                var relPath = XFilesystem.PathSubtract(emptyFolder, project.FullPath);

                var parts = XFilesystem.PathSplit(relPath);

                await unrealVS.AddSubpathToProjectAsync(parts);
            }
            */
            await unrealService.UpdateEmptyFoldersAsync();

            unrealVS.WhenProjectReload_MarkItemsForCreation = unrealService.KnownEmptyFolderPaths();
            watch.Stop();

            XDebug.Info($"Empty folder loaded on VS in {watch.ElapsedMilliseconds} ms");
            unrealVS.Output.Warn("DetectEmptyHandler - End");
        }
    }
}