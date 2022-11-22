using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;

namespace FUnreal
{
    public class DeleteGameModuleController : IXActionController
    {
        private FUnrealNotifier _notifier;
        private ConfirmDialog _dialog;

        public DeleteGameModuleController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _notifier = new FUnrealNotifier();
        }
/*
        public override async Task<bool> ShouldBeVisibleAsync()
        {
            bool isVisile = await base.ShouldBeVisibleAsync();
            if (isVisile)
            {
                var itemVs = await _unrealVS.GetSelectedItemAsync();
                if (_unrealService.IsPrimaryGameModulePath(itemVs.FullPath))
                {
                    Command.Enabled = false;   
                } 
            }
            return isVisile;
        }
*/

        public override async Task DoActionAsync()
        {
            var itemVs = await _unrealVS.GetSelectedItemAsync();
            string moduleName = _unrealService.ModuleNameFromSourceCodePath(itemVs.FullPath);

            if (!_unrealService.ExistsModule(moduleName))
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_ModuleNotExists);
                return;
            }

            _dialog = new ConfirmDialog("Selected module will be deleted permanently:", $"{moduleName}");
            _notifier.OnSendMessage = _dialog.SetProgressMessage;
            _dialog.OnConfirm = async () =>
            {
                _dialog.ShowActionInProgress();
                bool success = await _unrealService.DeleteGameModuleAsync(moduleName, _notifier);
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