using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.CommandBars;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public class AddPluginController : IXActionController
    {
        private AddPluginDialog _dialog;
        private string _lastPlugName;
        private List<FUnrealTemplate> _templates;
        private FUnrealNotifier _notifier;
        private bool _templateHasModuleName;

        public AddPluginController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS, ctxMenuMgr)
        {
            _templates = _unrealService.PluginTemplates();
            _notifier = new FUnrealNotifier();
        }

        public override async Task<bool> ShouldBeVisibleAsync()
        {
            bool active = await _ctxMenuMgr.IsActiveAsync(VSCTSymbols.AddPluginCmd);

            return active;
        }

        public override async Task DoActionAsync()
        {
            _lastPlugName = string.Empty;

            _dialog = new AddPluginDialog();
            _dialog.OnTemplateChangeAsync = TemplateChangedAsync;
            _dialog.OnPluginNameChangeAsync = PluginNameChangedAsync;
            _dialog.OnModuleNameChangeAsync = ModuleNameChangedAsync;
            _dialog.OnConfirmAsync = ConfirmAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;


            if (_templates.Count == 0)
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_TemplatesNotFound);
                return;
            }

            _dialog.pluginTemplCbx.ItemsSource = _templates;
            _dialog.pluginTemplCbx.SelectedIndex = 0;  //Fire Template Changed
            _dialog.pluginTemplTbl.Text = _templates[0].Description;

            await _dialog.ShowDialogAsync();
        }

        public Task TemplateChangedAsync()
        {
            int index = _dialog.pluginTemplCbx.SelectedIndex;
            FUnrealTemplate selected = _templates[index];
            _dialog.pluginTemplTbl.Text = selected.Description;


            _templateHasModuleName = selected.HasPlaceHolder("ModuleName");
            if (_templateHasModuleName)
            {
                _dialog.ShowModuleNameControls(true);
            }
            else //Example: Content Only Plugin has no ModuleName
            {
                _dialog.ShowModuleNameControls(false);
            }

            _dialog.pluginNameTbx.Focus();
            _dialog.pluginNameTbx.Text = "NewPlugin"; //Setting Text will fire TextChange event
            _dialog.pluginNameTbx.SelectionStart = 0;
            _dialog.pluginNameTbx.SelectionLength = _dialog.pluginNameTbx.Text.Length;
            _lastPlugName = _dialog.pluginNameTbx.Text;

            return Task.CompletedTask;
        }

        public Task PluginNameChangedAsync()
        {
            string plugName = _dialog.pluginNameTbx.Text;
            if (string.IsNullOrEmpty(plugName))
            {
                _dialog.addButton.IsEnabled = false;
            }
            else
            {
                _dialog.pluginPathTbl.Text = _unrealService.RelPluginPath(plugName);

                if (_unrealService.ExistsPlugin(plugName))
                {
                    _dialog.addButton.IsEnabled = false;
                    _dialog.ShowError(XDialogLib.ErrorMsg_PluginAlreadyExists);
                }
                else
                {
                    _dialog.addButton.IsEnabled = true;
                    _dialog.HideError();
                }
            }
            
            //Sync moduleName until is equal to pluginName
            if (_templateHasModuleName) { 
                string modName = _dialog.moduleNameTbx.Text;
                if (modName == _lastPlugName)
                {
                    _dialog.moduleNameTbx.Text = plugName; //Fire Text Changed on ModuleName
                }
                _lastPlugName = plugName;
            }

            return Task.CompletedTask;
        }

        public Task ModuleNameChangedAsync()
        {
            string plugName = _dialog.pluginNameTbx.Text;
            string modName = _dialog.moduleNameTbx.Text;

            _dialog.modulePathTbl.Text = _unrealService.RelPluginModulePath(plugName, modName);

            if (string.IsNullOrEmpty(modName))
            {
                _dialog.addButton.IsEnabled = false;
            } else
            {
                _dialog.addButton.IsEnabled = true;
            }

            return Task.CompletedTask;
        }

        public async Task ConfirmAsync()
        {
            _dialog.ShowActionInProgress();

            string pluginName = _dialog.pluginNameTbx.Text;
            string moduleNameOrNull = _templateHasModuleName ? _dialog.moduleNameTbx.Text : null;
            string templeName = _templates[_dialog.pluginTemplCbx.SelectedIndex].Name;

            bool success = await _unrealService.AddPluginAsync(templeName, pluginName, moduleNameOrNull, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }
            _dialog.Close();
        }
    }
}
