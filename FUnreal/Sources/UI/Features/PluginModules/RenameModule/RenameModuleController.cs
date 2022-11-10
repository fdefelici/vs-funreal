using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public class RenameModuleController : IXActionController
    {
        private RenameModuleDialog _dialog;
        private FUnrealNotifier _notifier;
        private string _moduleOriginalName;
        private string _pluginName;

        public RenameModuleController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS, ctxMenuMgr)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var itemVs = await _unrealVS.GetSelectedItemAsync();
            _pluginName = _unrealService.PluginNameFromSourceCodePath(itemVs.FullPath);
            _moduleOriginalName = _unrealService.ModuleNameFromSourceCodePath(itemVs.FullPath);

            if (!_unrealService.ExistsPluginModule(_pluginName, _moduleOriginalName))
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_ModuleNotExists);
                return;
            }

            _dialog = new RenameModuleDialog();
            _dialog.OnConfirmAsync = ConfirmAsync;
            _dialog.OnRenameAsync = ModuleNameChangedAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;

            _dialog.pluginNameTbx.Text = _pluginName;
            _dialog.pluginPathTbl.Text = _unrealService.RelPluginPath(_pluginName);
            _dialog.renameFilesCbx.IsChecked = false;

            _dialog.moduleNewNameTbx.Text = _moduleOriginalName; //Setting text fires TextChanged
            _dialog.moduleNewNameTbx.Focus();
            _dialog.moduleNewNameTbx.SelectionStart = 0;
            _dialog.moduleNewNameTbx.SelectionLength = _moduleOriginalName.Length;

            await _dialog.ShowDialogAsync();
        }

        public Task ModuleNameChangedAsync()
        {
            string moduleNewName = _dialog.moduleNewNameTbx.Text;

            _dialog.moduleNewPathTbl.Text = _unrealService.RelPluginModulePath(_pluginName, moduleNewName);

            bool IsValid = !string.IsNullOrEmpty(moduleNewName) 
                           && !_moduleOriginalName.Equals(moduleNewName);

            bool AlreadExists = IsValid && _unrealService.ExistsPluginModule(_pluginName, moduleNewName);

            if (AlreadExists)
            {
                _dialog.confirmBtn.IsEnabled = false;
                _dialog.ShowError(XDialogLib.ErrorMsg_ModuleAlreadyExists);
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

            string moduleNewName = _dialog.moduleNewNameTbx.Text;
            bool updateCppFiles = (bool)_dialog.renameFilesCbx.IsChecked;

            bool success = await _unrealService.RenameGameModuleAsync(_moduleOriginalName, moduleNewName, updateCppFiles, _notifier); //.ConfigureAwait(false);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }
            _dialog.Close();
        }

    }
}
