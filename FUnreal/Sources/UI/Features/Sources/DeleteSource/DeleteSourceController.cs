using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Text.Adornments;

namespace FUnreal
{
    public class DeleteSourceController : IXActionController
    {
        private FUnrealNotifier _notifier;
        private ConfirmDialog _dialog;
        List<FUnrealVSItem> _selectedItems;

        public DeleteSourceController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS, ctxMenuMgr)
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

            //Remove folder from VS just in case where empty folders exists (in this case ubt regeneration does't update Virtual Folder / Filters)
            await _unrealVS.RemoveFoldersIfAnyInCurrentSelectionAsync();

            bool success = await _unrealService.DeleteSourcesAsync(validSourcePaths, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }

            _dialog.Close();
        }
    }
}