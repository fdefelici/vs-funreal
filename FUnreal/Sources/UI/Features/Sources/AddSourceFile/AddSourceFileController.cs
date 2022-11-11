using Community.VisualStudio.Toolkit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public class AddSourceFileController : IXActionController
    {
        private AddSourceFileDialog _dialog;
        private FUnrealNotifier _notifier;
        private string _absPathSelected;
        private FUnrealSourceType _absPathSelectedType;

        public AddSourceFileController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS, ctxMenuMgr)
        {
            _notifier = new FUnrealNotifier();
        }

        public override Task<bool> ShouldBeVisibleAsync()
        {
            return base.ShouldBeVisibleAsync();
        }

        public override async Task DoActionAsync()
        {
            _dialog = new AddSourceFileDialog();
            _dialog.OnFileNameChangeAsync = FileNameChangedAsync;
            _dialog.OnConfirmAsync = ConfirmAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;

            var item = await _unrealVS.GetSelectedItemAsync();
            _absPathSelected = item.FullPath;
            _absPathSelectedType = _unrealService.TypeForSourcePath(_absPathSelected);
            if (_absPathSelectedType == FUnrealSourceType.INVALID)
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_InvalidPath, _absPathSelected);
                return;
            }

            _dialog.fileNameTbx.Focus();
            _dialog.fileNameTbx.Text = "NewFile.ext";  //Fire Name Changed
            _dialog.fileNameTbx.SelectionStart = 0;
            _dialog.fileNameTbx.SelectionLength = _dialog.fileNameTbx.Text.Length;

            await _dialog.ShowDialogAsync();
        }

        public Task FileNameChangedAsync()
        {
            string fileNameWithExt = _dialog.fileNameTbx.Text;

            string filePath = XFilesystem.PathCombine(_absPathSelected, fileNameWithExt);
           
            _dialog.filePathTbl.Text = _unrealService.RelPathToModule(filePath);

            if (string.IsNullOrEmpty(fileNameWithExt))
            {
                _dialog.addButton.IsEnabled = false;
            }
            else
            {
                if (XFilesystem.FileExists(filePath))
                {
                    _dialog.addButton.IsEnabled = false;
                    _dialog.ShowError(XDialogLib.ErrorMsg_FileAlreadyExists);
                }
                else
                {
                    _dialog.addButton.IsEnabled = true;
                    _dialog.HideError();
                }
            }
            return Task.CompletedTask;
        }

        public async Task ConfirmAsync()
        {
            _dialog.ShowActionInProgress();

            string absBasePath = _absPathSelected;
            string fileName = _dialog.fileNameTbx.Text;

            bool success = await _unrealService.AddSourceFileAsync(absBasePath, fileName, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }
            _dialog.Close();
        }
    }
}
