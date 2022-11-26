using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;

namespace FUnreal
{
    public class DeleteModuleController : AXActionController
    {
        private FUnrealNotifier _notifier;
        private ConfirmDialog _dialog;
        private string _pluginName;
        private string _moduleName;

        public DeleteModuleController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var itemVs = await _unrealVS.GetSelectedItemAsync();
            _pluginName = _unrealService.PluginNameFromSourceCodePath(itemVs.FullPath);
            _moduleName = _unrealService.ModuleNameFromSourceCodePath(itemVs.FullPath);

            bool moduleExists = _unrealService.ExistsModule(_moduleName);
            if (!moduleExists)
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_ModuleNotExists);
                return;
            }

            _dialog = new ConfirmDialog("Selected module will be deleted permanently:", $"{_pluginName}::{_moduleName}");
            _notifier.OnSendMessage = _dialog.SetProgressMessage;
            _dialog.OnConfirm = ConfirmAsync;

            await _dialog.ShowDialogAsync();
        }

        private async Task ConfirmAsync()
        {
            _dialog.ShowActionInProgress();
            var success = await _unrealService.DeletePluginModuleAsync(_pluginName, _moduleName, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }

            _unrealVS.WhenProjectReload_MarkItemForSelection = _unrealService.AbsPluginPath_RealOrTheorical(_pluginName);

            _dialog.Close();
        }
    }
}