using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;

namespace FUnreal
{
    public class DeletePluginController : AXActionController
    {

        private FUnrealNotifier _notifier;
        private ConfirmDialog _dialog;
        private string _pluginName;

        public DeletePluginController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var itemVs = await _unrealVS.GetSelectedItemAsync();
            _pluginName = _unrealService.PluginNameFromSourceCodePath(itemVs.FullPath);

            bool pluginExists = _unrealService.ExistsPlugin(_pluginName);
            if (!pluginExists)
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_PluginNotExists, _pluginName);
                return; 
            }

            _dialog = new ConfirmDialog(XDialogLib.InfoMsg_PluginDelete, _pluginName);
            _notifier.OnSendMessage = _dialog.SetProgressMessage;
            _dialog.OnConfirm = ConfirmAsync;

            await _dialog.ShowDialogAsync();
        }

        private async Task ConfirmAsync()
        {
            _dialog.ShowActionInProgress();
            var success = await _unrealService.DeletePluginAsync(_pluginName, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }

            _unrealVS.WhenProjectReload_MarkItemForSelection = _unrealService.AbsProjectPluginsPath();

            _dialog.Close();
        }
    }
}