using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;

namespace FUnreal
{
    public class DeleteGameModuleController : AXActionController
    {
        private FUnrealNotifier _notifier;
        private ConfirmDialog _dialog;
        private string _moduleName;

        public DeleteGameModuleController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var itemVs = await _unrealVS.GetSelectedItemAsync();
            _moduleName = _unrealService.ModuleNameFromSourceCodePath(itemVs.FullPath);

            if (!_unrealService.ExistsModule(_moduleName))
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_ModuleNotExists);
                return;
            }

            //TODO: Message to static const string
            _dialog = new ConfirmDialog("Selected module will be deleted permanently:", $"{_moduleName}");
            _notifier.OnSendMessage = _dialog.SetProgressMessage;
            _dialog.OnConfirm = ConfirmAsync;

            await _dialog.ShowDialogAsync();
        }

        private async Task ConfirmAsync()
        {
            _dialog.ShowActionInProgress();
            var success = await _unrealService.DeleteGameModuleAsync(_moduleName, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }

            _unrealVS.WhenProjectReload_MarkItemForSelection = _unrealService.AbsProjectSourcePath();
            
            _dialog.Close();
        }
    }
}