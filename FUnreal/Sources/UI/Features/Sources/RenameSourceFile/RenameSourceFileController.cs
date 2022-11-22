using Community.VisualStudio.Toolkit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public class RenameSourceFileController : IXActionController
    {
        private AddSourceFileDialog _dialog;
        private FUnrealNotifier _notifier;
        private string _absFilePath;
        private string _absBasePath;
        private string _originalFileName;

        public RenameSourceFileController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var item = await _unrealVS.GetSelectedItemAsync();
            _absFilePath = item.FullPath;
            if (!XFilesystem.FileExists(_absFilePath))
            {
                await XDialogLib.ShowErrorDialogAsync(XDialogLib.ErrorMsg_PathNotExists, _absFilePath);
                return;
            }

            _absBasePath = XFilesystem.PathParent(_absFilePath);
            _originalFileName = XFilesystem.GetFileNameWithExt(_absFilePath);

            _dialog = AddSourceFileDialog.CreateInRenameMode();
            _dialog.OnFileNameChangeAsync = FileNameChangedAsync;
            _dialog.OnConfirmAsync = ConfirmAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;

            _dialog.fileNameTbx.Focus();
            _dialog.fileNameTbx.Text = _originalFileName;  //Fire Name Changed
            _dialog.fileNameTbx.SelectionStart = 0;
            _dialog.fileNameTbx.SelectionLength = _dialog.fileNameTbx.Text.Length;

            await _dialog.ShowDialogAsync();
        }

        public Task FileNameChangedAsync()
        {
            string fileNameWithExt = _dialog.fileNameTbx.Text;

            string filePath = XFilesystem.PathCombine(_absBasePath, fileNameWithExt);
           
            _dialog.filePathTbl.Text = _unrealService.ModuleRelativePath(filePath);

            if (string.IsNullOrEmpty(fileNameWithExt))
            {
                _dialog.addButton.IsEnabled = false;
            }
            else if (fileNameWithExt == _originalFileName)
            {
                _dialog.addButton.IsEnabled = false;
            }
            else if (!XDialogLib.IsValidFileNameWitExt(fileNameWithExt))
            {
                _dialog.addButton.IsEnabled = false;
                _dialog.ShowError(XDialogLib.ErrorMsg_InvalidInput);
            }
            else if (XFilesystem.FileExists(filePath))
            {
                _dialog.addButton.IsEnabled = false;
                _dialog.ShowError(XDialogLib.ErrorMsg_FileAlreadyExists);
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

            string newFileNameWithExt = _dialog.fileNameTbx.Text;

            bool success = await _unrealService.RenameFileAsync(_absFilePath, newFileNameWithExt, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }
            _dialog.Close();
        }
    }
}
