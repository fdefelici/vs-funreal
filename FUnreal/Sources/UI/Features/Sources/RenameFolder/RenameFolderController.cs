using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;


using Microsoft.VisualStudio.Shell;

namespace FUnreal
{
    public class RenameFolderController : AXActionController
    {
        private AddFolderDialog _dialog;
        private FUnrealNotifier _notifier;
        private string _absPathSelected;
        private string _originalFolderName;

        public RenameFolderController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            var item = await _unrealVS.GetSelectedItemAsync();
            _absPathSelected = item.FullPath;
            if (!XFilesystem.DirExists(_absPathSelected))
            {
                await XDialogLib.ShowErrorDialogAsync(XDialogLib.ErrorMsg_PathNotExists, _absPathSelected);
                return;
            }
            _originalFolderName = XFilesystem.GetLastPathToken(_absPathSelected);

            _dialog = AddFolderDialog.CreateInRenameMode();
            _dialog.OnPathChangeAsync = PathChangedAsync;
            _dialog.OnConfirmAsync = ConfirmAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;

            _dialog.pathTbx.Focus();
            _dialog.pathTbx.Text = _originalFolderName;
            _dialog.pathTbx.SelectionStart = 0;
            _dialog.pathTbx.SelectionLength = _dialog.pathTbx.Text.Length;

            await _dialog.ShowDialogAsync();
        }

        public async Task PathChangedAsync()
        {
            string inputPath = _dialog.pathTbx.Text;
            string newFolderPath = XFilesystem.ChangeDirName(_absPathSelected, inputPath);
            
            _dialog.folderPathTbl.Text = _unrealService.ModuleRelativePath(newFolderPath);

            if (string.IsNullOrEmpty(inputPath))
            {
                _dialog.addButton.IsEnabled = false;
            } 
            else if (inputPath == _originalFolderName)
            { 
                _dialog.addButton.IsEnabled = false;
            }
            else if (XFilesystem.DirExists(newFolderPath) || await _unrealVS.ExistsFolderInSelectedFolderParentAsync(inputPath))
            //else if (await _unrealVS.ExistsSubpathFromSelectedFolderAsync(parts)) //To allow creating VS Virtual Folder / Filter in case of existent folder on Filsystem
            {
                _dialog.addButton.IsEnabled = false;
                _dialog.ShowError(XDialogLib.ErrorMsg_FolderAlreadyExists);
            }
            else
            {
                _dialog.addButton.IsEnabled = true;
                _dialog.HideError();
            }
        }

        public async Task ConfirmAsync()
        {
            _dialog.ShowActionInProgress();

            string fullPath = _absPathSelected;
            string newDirName = _dialog.pathTbx.Text;

            var success = await _unrealService.RenameFolderAsync(fullPath, newDirName, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }

            _unrealVS.WhenProjectReload_MarkItemForSelection = success.FilePath;


            _notifier.OnSendMessage = null;
            _dialog.Close();
        }
    }
}
