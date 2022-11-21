using Community.VisualStudio.Toolkit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public class AddGameModuleController : IXActionController
    {
        private List<FUnrealTemplate> _templates;
        private FUnrealNotifier _notifier;
        private AddGameModuleDialog _dialog;

        public AddGameModuleController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS, ctxMenuMgr)
        {
            _templates = _unrealService.ModuleTemplates();
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var itemVs = await _unrealVS.GetSelectedItemAsync();


            _dialog = new AddGameModuleDialog();
            _dialog.OnConfirmAsync = ConfirmAsync;
            _dialog.OnTemplateChangeAsync = TemplateChangedAsync;
            _dialog.OnModuleNameChangeAsync = ModuleNameChangedAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;

            //NOTE: doing this as last operation because it will fire templatechange event
            _dialog.templateCbx.ItemsSource = _templates;
            _dialog.templateCbx.SelectedIndex = 0;   //fire TemplateChanged Event
            _dialog.templateTbl.Text = _templates[0].Description;

            await _dialog.ShowDialogAsync();
        }

        public async Task ConfirmAsync()
        {
            _dialog.ShowActionInProgress();
            
            string moduleName = _dialog.moduleNameTbx.Text;
            string templeName = _templates[_dialog.templateCbx.SelectedIndex].Name;

            bool success = await _unrealService.AddGameModuleAsync(templeName, moduleName, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }

            //if false, show error message;
            _dialog.Close();
        }

        public Task TemplateChangedAsync()
        {
            int index = _dialog.templateCbx.SelectedIndex;
            FUnrealTemplate selected = _templates[index];
            _dialog.templateTbl.Text = selected.Description;

            _dialog.moduleNameTbx.Focus();
            _dialog.moduleNameTbx.Text = "NewGameModule"; //Setting Text will fire TextChange event
            _dialog.moduleNameTbx.SelectionStart = 0;
            _dialog.moduleNameTbx.SelectionLength = _dialog.moduleNameTbx.Text.Length;

            return Task.CompletedTask;
        }

        public Task ModuleNameChangedAsync()
        {
            string modName = _dialog.moduleNameTbx.Text;

            _dialog.modulePathTbl.Text = _unrealService.ProjectRelativePathForGameModuleDefault(modName);

            if (string.IsNullOrEmpty(modName))
            {
                _dialog.addButton.IsEnabled = false;
            } 
            else if (_unrealService.ExistsModule(modName))
            {
                _dialog.addButton.IsEnabled = false;
                _dialog.ShowError(XDialogLib.ErrorMsg_ModuleAlreadyExists, _unrealService.ProjectRelativePathForModule(modName));
            }
            else
            {
                _dialog.addButton.IsEnabled = true;
                _dialog.HideError();
            }
            return Task.CompletedTask;
        }
    }
}
