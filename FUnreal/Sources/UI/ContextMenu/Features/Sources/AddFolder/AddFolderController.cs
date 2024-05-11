using System.Threading.Tasks;
using System.Windows;


using Microsoft.VisualStudio.Shell;

namespace FUnreal
{
    public class AddFolderController : AXActionController
    {
        private AddFolderDialog _dialog;
        private FUnrealNotifier _notifier;
        private string _absPathSelected;

        public AddFolderController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            //Add, it is able to create path, even if selected path doesn't exists on disk

            _dialog = new AddFolderDialog();
            _dialog.OnPathChangeAsync = PathChangedAsync;
            _dialog.OnConfirmAsync = ConfirmAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;

            var item = await _unrealVS.GetSelectedItemAsync();
            _absPathSelected = item.FullPath;

            _dialog.pathTbx.Focus();
            _dialog.pathTbx.Text = "NewFolder";  //Fire Name Changed
            _dialog.pathTbx.SelectionStart = 0;
            _dialog.pathTbx.SelectionLength = _dialog.pathTbx.Text.Length;

            await _dialog.ShowDialogAsync();
        }

        public async Task PathChangedAsync()
        {
            string inputPath = _dialog.pathTbx.Text;
            string folderPath = XFilesystem.PathCombine(_absPathSelected, inputPath);
            
            _dialog.folderPathTbl.Text = _unrealService.ModuleRelativePath(folderPath);

             string[] parts = XFilesystem.PathSplit(inputPath);

            if (string.IsNullOrEmpty(inputPath))
            {
                _dialog.addButton.IsEnabled = false;
            } 
            //else if (XFilesystem.DirectoryExists(folderPath) || await _unrealVS.ExistsSubpathFromSelectedFolderAsync(parts))
            else if (await _unrealVS.ExistsSubpathFromSelectedFolderAsync(parts)) //To allow creating VS Virtual Folder / Filter in case of existent folder on Filsystem
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

            string absBasePath = _absPathSelected;
            string inputPath = _dialog.pathTbx.Text;
            inputPath = XFilesystem.PathFixSeparator(inputPath, true);

            _notifier.Info("Creating path ...");
            string[] parts = XFilesystem.PathSplit(inputPath);
            bool success = await _unrealVS.AddSubpathToSelectedFolderAsync(parts);
            if (!success)
            {
                _notifier.Erro("Creating path ...", "Failed creating folder items in VS for {0}", inputPath);
                _dialog.ShowActionInError();
                return;
            }

            string fullPath = XFilesystem.PathCombine(absBasePath, inputPath);
            success = XFilesystem.DirCreate(fullPath);
            if (!success)
            {
                _notifier.Erro("Creating path ...", "Failed creating folder on filesystem {0}", fullPath);
                _dialog.ShowActionInError();
                return;
            }

            _unrealVS.WhenProjectReload_MarkItemForSelection = fullPath;

            _dialog.Close();
        }
    }
}
