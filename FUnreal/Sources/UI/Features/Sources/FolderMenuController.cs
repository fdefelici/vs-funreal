namespace FUnreal
{
    public class FolderMenuController : IXMenuController
    {
        private FUnrealService _unrealService;
        private FUnrealVS _unrealVS;

        public FolderMenuController(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            _unrealService = unrealService;
            _unrealVS = unrealVS;
        }

        public bool ShouldBeVisible()
        {
            /*
            var item = _unrealVS.GetSelectedItemAsync().GetAwaiter().GetResult();

            if (!item.IsVirtualFolder) return false;
            if (item.ProjectName != _unrealService.ProjectName) return false;

            if (!_unrealService.IsSourceCodePath(item.FullPath)) return false;

            return true;
            */

            //Visible in case of Multiple folder with files
            var itemsVs = _unrealVS.GetSelectedItemsAsync().GetAwaiter().GetResult();

            bool atLeastOneFolder = false;
            foreach (var item in itemsVs)
            {
                atLeastOneFolder |= item.IsVirtualFolder;

                if (!_unrealService.IsSourceCodePath(item.FullPath, true))
                {
                    return false;
                }
            }

            return atLeastOneFolder;
        }
    }
}