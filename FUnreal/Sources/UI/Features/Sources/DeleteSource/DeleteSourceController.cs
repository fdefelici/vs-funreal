using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using Community.VisualStudio.Toolkit;

namespace FUnreal
{
    public class DeleteSourceController : IXActionController
    {
        private FUnrealService _unrealService;
        private FUnrealVS _unrealVs;
        private FUnrealNotifier _notifier;
        private ConfirmDialog _dialog;
        public DeleteSourceController(FUnrealService unrealService, FUnrealVS unrealVs)
        {
            _unrealService = unrealService;
            _unrealVs = unrealVs;
            _notifier = new FUnrealNotifier();
            _notifier.OnSendMessage = (type, shortMsg, longMsg) =>
            {
                _dialog.SetProgressMessage(type, shortMsg, longMsg);
            };
        }

        //Visible only if it is source code path folder and it is not a Module folder
        public override bool ShouldBeVisible()
        {
            /*
            var itemVs = _unrealVs.GetSelectedItemAsync().GetAwaiter().GetResult();

            if (!_unrealService.IsSourceCodePath(itemVs.FullPath, true))
            {
                return false;
            }
            return true;

            var itemsVs = _unrealVs.GetSelectedItemsAsync().GetAwaiter().GetResult();

            foreach(var item in itemsVs)
            {
                if (!_unrealService.IsSourceCodePath(item.FullPath, true))
                {
                    return false;
                }
            }

            */
            return true;
        }

        public override async Task DoActionAsync()
        {
            var itemsVs = await _unrealVs.GetSelectedItemsAsync(); //cache when check in shouldbevisible

            List<string> sourcePaths = new List<string>();
            foreach (var item in itemsVs)
            {
                string itemPath = item.FullPath;
                if (!_unrealService.ExistsSourceDirectory(item.FullPath))
                {
                    await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_SourcePathNotFound, itemPath);
                    return;
                }
                sourcePaths.Add(itemPath);
            }

            string detailMsg;
            if (sourcePaths.Count == 1)
            {
                detailMsg = _unrealService.RelPathToModule(sourcePaths[0]);
            }
            else
            {
                detailMsg = $"{sourcePaths.Count} paths selected";
            }

            _dialog = new ConfirmDialog(XDialogLib.InfoMsg_SourcePathDelete, detailMsg);
            _dialog.OnConfirm = async () =>
            {
                _dialog.ShowActionInProgress();
                bool success = await _unrealService.DeleteSourceDirectoryAsync(sourcePaths, _notifier);
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