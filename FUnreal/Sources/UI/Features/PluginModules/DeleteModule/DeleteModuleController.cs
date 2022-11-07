using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;

namespace FUnreal
{
    public class DeleteModuleController : IXActionController
    {
        private FUnrealService _unrealService;
        private FUnrealVS _unrealVS;
        private FUnrealNotifier _notifier;
        private ConfirmDialog _dialog;
        public DeleteModuleController(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            _unrealService = unrealService;
            _unrealVS = unrealVS;
            _notifier = new FUnrealNotifier();
            _notifier.OnSendMessage = (type, shortMsg, detailedMsg) =>
            {
                _dialog.SetProgressMessage(type, shortMsg, detailedMsg);
            };
        }

        public override bool ShouldBeVisible()
        {
            return _unrealVS.IsSingleSelectionAsync().GetAwaiter().GetResult();
        }

        public override async Task DoActionAsync()
        {
            /*
            FUnrealVSPluginModule fmodule = await _unrealVS.GetSelectedPluginModuleAsync();
            string pluginName = fmodule.PluginName;
            string moduleName = fmodule.ModuleName;
            */

            var itemVs = await _unrealVS.GetSelectedItemAsync();
            string pluginName = _unrealService.PluginNameFromSourceCodePath(itemVs.FullPath);
            string moduleName = _unrealService.ModuleNameFromSourceCodePath(itemVs.FullPath);

            bool moduleExists = _unrealService.ExistsPluginModule(pluginName, moduleName);
            if (!moduleExists)
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_ModuleNotExists);
                return;
            }

            _dialog = new ConfirmDialog("Selected module will be deleted permanently:", $"{pluginName}::{moduleName}");
            _dialog.OnConfirm = async () =>
            {
                _dialog.ShowActionInProgress();
                bool success = await _unrealService.DeletePluginModuleAsync(pluginName, moduleName, _notifier);
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