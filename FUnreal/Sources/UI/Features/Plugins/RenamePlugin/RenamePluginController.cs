using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public class RenamePluginController : IXActionController
    {
        private RenamePluginDialog _dialog;
        private FUnrealNotifier _notifier;
        private string _pluginOriginalName;

        public RenamePluginController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS, ctxMenuMgr)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var itemVs = await _unrealVS.GetSelectedItemAsync();
            _pluginOriginalName = _unrealService.PluginNameFromSourceCodePath(itemVs.FullPath);

            if (!_unrealService.ExistsPlugin(_pluginOriginalName))
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_PluginNotExists);
                return;
            }

            _dialog = new RenamePluginDialog();
            _dialog.OnConfirmAsync = ConfirmAsync;
            _dialog.OnRenameAsync = PluginNameChangedAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;

            _dialog.pluginNameTbx.Text = _pluginOriginalName;
            _dialog.pluginPathTbl.Text = _unrealService.ProjectRelativePathForPlugin(_pluginOriginalName);
            _dialog.pluginNewNameTbx.Text = _pluginOriginalName; //Setting text fires TextChanged
            _dialog.pluginNewNameTbx.SelectionStart = 0;
            _dialog.pluginNewNameTbx.SelectionLength = _pluginOriginalName.Length;

            await _dialog.ShowDialogAsync();
        }

        private Task PluginNameChangedAsync()
        {
            string pluginNewName = _dialog.pluginNewNameTbx.Text;

            _dialog.pluginNewPathTbl.Text = _unrealService.ProjectRelativePathForPlugin(pluginNewName);

            bool IsValid = !string.IsNullOrEmpty(pluginNewName) 
                           && !_pluginOriginalName.Equals(pluginNewName);

            bool AlreadExists = IsValid && _unrealService.ExistsPlugin(pluginNewName);

            if (AlreadExists)
            {
                _dialog.confirmBtn.IsEnabled = false;
                _dialog.ShowError(XDialogLib.ErrorMsg_PluginAlreadyExists);
            }
            else if (!IsValid)
            {
                _dialog.confirmBtn.IsEnabled = false;
                _dialog.HideError();
            }
            else
            {
                _dialog.confirmBtn.IsEnabled = true;
                _dialog.HideError();
            }
            return Task.CompletedTask;
        }

        private async Task ConfirmAsync()
        {
            _dialog.ShowActionInProgress();

            string pluginNewName = _dialog.pluginNewNameTbx.Text;

            bool success = await _unrealService.RenamePluginAsync(_pluginOriginalName, pluginNewName, _notifier); //.ConfigureAwait(false);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }
            _dialog.Close();
        }
    }
}
