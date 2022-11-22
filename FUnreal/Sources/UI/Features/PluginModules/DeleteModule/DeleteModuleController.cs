using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;

namespace FUnreal
{
    public class DeleteModuleController : IXActionController
    {
        private FUnrealNotifier _notifier;
        private ConfirmDialog _dialog;

        public DeleteModuleController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var itemVs = await _unrealVS.GetSelectedItemAsync();
            string pluginName = _unrealService.PluginNameFromSourceCodePath(itemVs.FullPath);
            string moduleName = _unrealService.ModuleNameFromSourceCodePath(itemVs.FullPath);

            bool moduleExists = _unrealService.ExistsModule(moduleName);
            if (!moduleExists)
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_ModuleNotExists);
                return;
            }

            _dialog = new ConfirmDialog("Selected module will be deleted permanently:", $"{pluginName}::{moduleName}");
            _notifier.OnSendMessage = _dialog.SetProgressMessage;
            _dialog.OnConfirm = async () =>
            {
                _dialog.ShowActionInProgress();
                bool success = await _unrealService.DeletePluginModuleAsync(pluginName, moduleName, _notifier);
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