namespace FUnreal
{
    public class SourceFileMenuController : IXMenuController
    {
        private FUnrealService _unrealService;
        private FUnrealVS _unrealVS;

        public SourceFileMenuController(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            _unrealService = unrealService;
            _unrealVS = unrealVS;
        }

        public bool ShouldBeVisible()
        {
            var itemsVs = _unrealVS.GetSelectedItemsAsync().GetAwaiter().GetResult();
            
            foreach(var item in itemsVs)
            {
                if (!item.IsFile) return false;
                if (item.ProjectName != _unrealService.ProjectName) return false;
                if (!_unrealService.IsSourceCodePath(item.FullPath)) return false;
            }

            return true;
        }
    }
}