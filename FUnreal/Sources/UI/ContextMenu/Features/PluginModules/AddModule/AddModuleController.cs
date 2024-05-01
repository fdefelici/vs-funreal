using Community.VisualStudio.Toolkit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public class AddModuleController : AXActionController
    {
        private List<FUnrealPluginModuleTemplate> _templates;
        private FUnrealNotifier _notifier;
        private string _currentPluginName;
        private AddModuleDialog _dialog;

        public AddModuleController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _templates = _unrealService.PluginModuleTemplates();
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var itemVs = await _unrealVS.GetSelectedItemAsync();
            _currentPluginName = _unrealService.PluginNameFromSourceCodePath(itemVs.FullPath);


            _dialog = new AddModuleDialog();
            _dialog.OnConfirmAsync = ConfirmAsync;
            _dialog.OnTemplateChangeAsync = TemplateChangedAsync;
            _dialog.OnModuleNameChangeAsync = ModuleNameChangedAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;

            _dialog.pluginNameTbl.Text = _currentPluginName;
            _dialog.pluginPathTbl.Text = _unrealService.ProjectRelativePathForPlugin(_currentPluginName);

            //NOTE: doing this as last operation because it will fire templatechange event
            _dialog.pluginTemplCbx.ItemsSource = _templates;
            _dialog.pluginTemplCbx.SelectedIndex = 0;   //fire TemplateChanged Event
            _dialog.pluginTemplTbl.Text = _templates[0].Description;

            await _dialog.ShowDialogAsync();
        }


        public Task TemplateChangedAsync()
        {
            int index = _dialog.pluginTemplCbx.SelectedIndex;
            var selected = _templates[index];
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

            _dialog.modulePathTbl.Text = _unrealService.ProjectRelativePathForPluginModuleDefault(plugName, modName);

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

        public async Task ConfirmAsync()
        {
            _dialog.ShowActionInProgress();

            string pluginName = _currentPluginName;
            string moduleName = _dialog.moduleNameTbx.Text;
            string templeName = _templates[_dialog.pluginTemplCbx.SelectedIndex].Name;

            var success = await _unrealService.AddPluginModuleAsync(templeName, pluginName, moduleName, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }

            _unrealVS.WhenProjectReload_MarkItemForSelection = success.BuildFilePath;

            _dialog.Close();
        }

    }
}
