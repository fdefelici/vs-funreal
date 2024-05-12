using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;

namespace FUnreal
{
    public class DeleteSourceController : AXActionController
    {
        private FUnrealNotifier _notifier;
        private ConfirmDialog _dialog;
        List<FUnrealVSItem> _selectedItems;

        public DeleteSourceController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            _selectedItems = await _unrealVS.GetSelectedItemsAsync();

            string detailMsg;
            if (_selectedItems.Count == 1)
            {
                detailMsg = _unrealService.ModuleRelativePath(_selectedItems[0].FullPath);
            }
            else
            {
                detailMsg = $"{_selectedItems.Count} paths selected";
            }

            _dialog = new ConfirmDialog(XDialogLib.InfoMsg_SourcePathDelete, detailMsg);
            _notifier.OnSendMessage = _dialog.SetProgressMessage;
            _dialog.OnConfirm = DeleteConfirmedAsync;

            await _dialog.ShowDialogAsync();
        }

        private async Task DeleteConfirmedAsync()
        {
            _dialog.ShowActionInProgress();

            var validSourcePaths = new List<string>();
            foreach (var item in _selectedItems)
            {
                string itemPath = item.FullPath;

                bool isValidFile = item.IsFile && _unrealService.ExistsSourceFile(item.FullPath);
                bool isValidFolder = item.IsVirtualFolder && _unrealService.ExistsSourceDirectory(item.FullPath);

                if (isValidFile || isValidFolder)
                {
                    validSourcePaths.Add(itemPath);
                }
                else //Item only present in VS Solution Explorer
                {
                    //In case item not exists on filesystem, remove it silently from VS view
                    string relPath = _unrealService.ProjectRelativePath(item.FullPath, false);
                    await _unrealVS.RemoveProjectItemByRelPathAsync(relPath);
                }
            }

            //Remove all folders within Selection from VS Solution Folder just in case where empty folders exists
            //(in this case ubt regeneration does't update Virtual Folder / Filters if selection is made only from empty folders)
            await _unrealVS.RemoveFoldersIfAnyInCurrentSelectionAsync();

            var success = await _unrealService.DeleteSourcesAsync(validSourcePaths, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }

            if (success.AllPaths.Count == 1)
            {
                _unrealVS.WhenProjectReload_MarkItemForSelection = XFilesystem.PathParent(success.AllPaths[0]);
            }
            else if (success.AllPaths.Count > 1 && success.AllParentPaths.Count > 0)
            {
                _unrealVS.WhenProjectReload_MarkItemForSelection = success.AllParentPaths[0];//XFilesystem.SelectCommonBasePath(success.AllParentPaths);
            }

            _dialog.Close();
        }
    }
}