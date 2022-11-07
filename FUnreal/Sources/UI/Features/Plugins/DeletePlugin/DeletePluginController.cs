using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;

namespace FUnreal
{
    public class DeletePluginController : IXActionController
    {
        private FUnrealService _unrealService;
        private FUnrealVS _unrealVs;
        private FUnrealNotifier _notifier;
        private ConfirmDialog _dialog;
        public DeletePluginController(FUnrealService unrealService, FUnrealVS unrealVs)
        {
            _unrealService = unrealService;
            _unrealVs = unrealVs;
            _notifier = new FUnrealNotifier();
            _notifier.OnSendMessage = (type, shortMsg, longMsg) =>
            {
                _dialog.SetProgressMessage(type, shortMsg, longMsg);
            };
        }

        public override async Task DoActionAsync()
        {
            var pluginVs = await _unrealVs.GetSelectedPluginAsync();
            string pluginName = pluginVs.PluginName;

            bool pluginExists = _unrealService.ExistsPlugin(pluginName);
            if (!pluginExists)
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_PluginNotExists, pluginName);
                return; 
            }

            _dialog = new ConfirmDialog(XDialogLib.InfoMsg_PluginDelete, pluginName);
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