namespace FUnreal
{
    public class PluginModuleMenuController : IXMenuController
    {
        private FUnrealService _unrealService;
        private FUnrealVS _unrealVS;

        public PluginModuleMenuController(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            _unrealService = unrealService;
            _unrealVS = unrealVS;
        }

        public bool ShouldBeVisible()
        {
            var itemsVs = _unrealVS.GetSelectedItemsAsync().GetAwaiter().GetResult();

            foreach (var item in itemsVs)
            {
                if (!_unrealService.IsModulePathOrTargetFile(item.FullPath)) return false;
            }

            return true;
        }
    }
}