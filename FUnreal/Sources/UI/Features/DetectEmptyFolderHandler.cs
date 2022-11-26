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

#if DEBUG
            //unrealVS.Output.Warn("DetectEmptyHandler - Start");
            Stopwatch watch = Stopwatch.StartNew();
            XDebug.Info("Discovering empty folders ...");
#endif

            await unrealService.UpdateEmptyFoldersAsync();
#if DEBUG
            unrealVS.WhenProjectReload_MarkItemsForCreation = unrealService.KnownEmptyFolderPaths();
            watch.Stop();

            XDebug.Info($"Empty folder loaded on VS in {watch.ElapsedMilliseconds} ms");
            //unrealVS.Output.Warn("DetectEmptyHandler - End");
#endif
        }
    }
}