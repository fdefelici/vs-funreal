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

        public DeleteSourceController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS, ctxMenuMgr)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var itemsVs = await _unrealVS.GetSelectedItemsAsync();

            List<string> sourcePaths = new List<string>();
            foreach (var item in itemsVs)
            {
                string itemPath = item.FullPath;

                bool isValidFile = item.IsFile && _unrealService.ExistsSourceFile(item.FullPath);
                bool isValidFolder = item.IsVirtualFolder && _unrealService.ExistsSourceDirectory(item.FullPath);

                if (isValidFile || isValidFolder)
                {
                    sourcePaths.Add(itemPath);
                }
                else
                {
                    await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_SourcePathNotFound, itemPath);
                    return;
                }
            }

            string detailMsg;
            if (sourcePaths.Count == 1)
            {
                detailMsg = _unrealService.ModuleRelativePath(sourcePaths[0]);
            }
            else
            {
                detailMsg = $"{sourcePaths.Count} paths selected";
            }

            _dialog = new ConfirmDialog(XDialogLib.InfoMsg_SourcePathDelete, detailMsg);
            _notifier.OnSendMessage = _dialog.SetProgressMessage;
            _dialog.OnConfirm = async () =>
            {
                _dialog.ShowActionInProgress();
                bool success = await _unrealService.DeleteSourcesAsync(sourcePaths, _notifier);
                if (!success)
                {
                    _dialog.ShowActionInError();
                } else
                {
                    _dialog.Close();
                }
            };

            await _dialog.ShowDialogAsync();
        }
    }
}