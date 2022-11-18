using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public class RenameGameModuleController : IXActionController
    {
        private RenameGameModuleDialog _dialog;
        private FUnrealNotifier _notifier;
        private string _moduleOriginalName;

        public RenameGameModuleController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS, ctxMenuMgr)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var itemVs = await _unrealVS.GetSelectedItemAsync();
            _moduleOriginalName = _unrealService.ModuleNameFromSourceCodePath(itemVs.FullPath);

            if (!_unrealService.ExistsModule(_moduleOriginalName))
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_ModuleNotExists);
                return;
            }

            _dialog = new RenameGameModuleDialog();
            _dialog.OnConfirmAsync = ConfirmAsync;
            _dialog.OnRenameAsync = ModuleNameChangedAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;

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

            _dialog.moduleNewPathTbl.Text = _unrealService.ProjectRelativePathForGameModuleDefault(moduleNewName);

            bool IsValid = !string.IsNullOrEmpty(moduleNewName) 
                           && !_moduleOriginalName.Equals(moduleNewName);

            bool AlreadExists = IsValid && _unrealService.ExistsModule(moduleNewName);

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
