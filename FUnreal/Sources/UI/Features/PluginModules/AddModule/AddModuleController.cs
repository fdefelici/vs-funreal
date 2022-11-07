using Community.VisualStudio.Toolkit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public class AddModuleController : IXActionController
    {
        private FUnrealService _unrealService;
        private FUnrealVS _unrealVs;
        private List<FUnrealTemplate> _templates;
        private FUnrealNotifier _notifier;
        private string _currentPluginName;
        private AddModuleDialog _dialog;

        public AddModuleController(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            _unrealService = unrealService;
            _unrealVs = unrealVS;
            _templates = _unrealService.ModuleTemplates();
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            _dialog = new AddModuleDialog();
            _dialog.OnConfirmAsync = ConfirmAsync;
            _dialog.OnTemplateChangeAsync = TemplateChangedAsync;
            _dialog.OnModuleNameChangeAsync = ModuleNameChangedAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;

            if (_templates.Count == 0)
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_TemplatesNotFound);
                return;
            }

            var pluginVs = await _unrealVs.GetSelectedPluginAsync();
            _currentPluginName = pluginVs.PluginName;

            _dialog.pluginNameTbl.Text = _currentPluginName;
            _dialog.pluginPathTbl.Text = _unrealService.RelPluginPath(_currentPluginName);


            //NOTE: doing this as last operation because it will fire templatechange event
            _dialog.pluginTemplCbx.ItemsSource = _templates;
            _dialog.pluginTemplCbx.SelectedIndex = 0;   //fire TemplateChanged Event
            _dialog.pluginTemplTbl.Text = _templates[0].Description;

            await _dialog.ShowDialogAsync();
        }

        public async Task ConfirmAsync()
        {
            _dialog.ShowActionInProgress();
            
            string pluginName = _currentPluginName;
            string moduleName = _dialog.moduleNameTbx.Text;
            string templeName = _templates[_dialog.pluginTemplCbx.SelectedIndex].Name;

            bool success = await _unrealService.AddModuleAsync(templeName, pluginName, moduleName, _notifier);
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
            int index = _dialog.pluginTemplCbx.SelectedIndex;
            FUnrealTemplate selected = _templates[index];
            _dialog.pluginTemplTbl.Text = selected.Description;

            _dialog.moduleNameTbx.Focus();
            _dialog.moduleNameTbx.Text = "NewModule"; //Setting Text will fire TextChange event
            _dialog.moduleNameTbx.SelectionStart = 0;
            _dialog.moduleNameTbx.SelectionLength = _dialog.moduleNameTbx.Text.Length;

            return Task.CompletedTask;
        }

        public Task ModuleNameChangedAsync()
        {
            string plugName = _currentPluginName;
            string modName = _dialog.moduleNameTbx.Text;

            _dialog.modulePathTbl.Text = _unrealService.RelPluginModulePath(plugName, modName);

            if (string.IsNullOrEmpty(modName))
            {
                _dialog.addButton.IsEnabled = false;
            } 
            else if (_unrealService.ExistsPluginModule(plugName, modName))
            {
                _dialog.addButton.IsEnabled = false;
                _dialog.ShowError(AddPluginDialog.ErrorMsg_PluginAlreadyExists);
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
