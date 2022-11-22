using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;

namespace FUnreal
{
    public class DeletePluginController : IXActionController
    {

        private FUnrealNotifier _notifier;
        private ConfirmDialog _dialog;

        public DeletePluginController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var itemVs = await _unrealVS.GetSelectedItemAsync();
            string pluginName = _unrealService.PluginNameFromSourceCodePath(itemVs.FullPath);

            bool pluginExists = _unrealService.ExistsPlugin(pluginName);
            if (!pluginExists)
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_PluginNotExists, pluginName);
                return; 
            }

            _dialog = new ConfirmDialog(XDialogLib.InfoMsg_PluginDelete, pluginName);
            _notifier.OnSendMessage = _dialog.SetProgressMessage;
            _dialog.OnConfirm = async () =>
            {
                _dialog.ShowActionInProgress();
                bool success = await _unrealService.DeletePluginAsync(pluginName, _notifier);
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