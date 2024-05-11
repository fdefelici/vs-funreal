using Community.VisualStudio.Toolkit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public class AddSourceClassController : AXActionController
    {
        private AddSourceClassDialog _dialog;
        private List<FUnrealSourceTemplate> _templates;
        private FUnrealNotifier _notifier;
        private string _absPathSelected;
        private FUnrealSourceType _absPathSelectedType;

        public AddSourceClassController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS)
        {
            _notifier = new FUnrealNotifier();
        }

        public override async Task DoActionAsync()
        {
            _templates = _unrealService.SourceTemplates();

            _dialog = new AddSourceClassDialog();
            _dialog.OnTemplateChangeAsync = TemplateChangedAsync;
            _dialog.OnClassNameChangeAsync = ClassNameChangedAsync;
            _dialog.OnClassTypeChangeAsync = ClassTypeChangedAsync;
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

            if (_unrealService.ModulePathFromSourceCodePath(_absPathSelected) == _absPathSelected)
            {
                _dialog.ShowClasseTypeCbx(true, true, true);
            } 
            else if (_absPathSelectedType == FUnrealSourceType.PUBLIC || _absPathSelectedType == FUnrealSourceType.PRIVATE)
            {
                //if type is Public or Private, then Custom type checkbox disappear because is no sense
                _dialog.ShowClasseTypeCbx(true, true, false);
            }
            else //CUSTOM
            {
                _dialog.ShowClasseTypeCbx(false, false, true);
            }
           
            _dialog.classTemplCbx.ItemsSource = _templates;
            _dialog.classTemplCbx.SelectedIndex = 0;  //Fire Template Changed
            _dialog.classTemplTbl.Text = _templates[0].Description;

            await _dialog.ShowDialogAsync();
        }

        public Task TemplateChangedAsync()
        {
            int index = _dialog.classTemplCbx.SelectedIndex;
            var selected = _templates[index];
            _dialog.classTemplTbl.Text = selected.Description;

            _dialog.SetClassTypeIndex((int)_absPathSelectedType); //Fire Event

            _dialog.classNameTbx.Focus();
            _dialog.classNameTbx.Text = "NewClass"; //Setting Text will fire TextChange event
            _dialog.classNameTbx.SelectionStart = 0;
            _dialog.classNameTbx.SelectionLength = _dialog.classNameTbx.Text.Length;

            return Task.CompletedTask;
        }

        public Task ClassNameChangedAsync()
        {
            string className = _dialog.classNameTbx.Text;
            int typeIndex = _dialog.GetClassTypeIndex();
            FUnrealSourceType sourceType = (FUnrealSourceType)typeIndex;

            _unrealService.ComputeSourceCodePaths(_absPathSelected, className, sourceType, out string headerPath, out string sourcePath, out _);

            _dialog.headerPathTbl.Text = _unrealService.ModuleRelativePath(headerPath);
            _dialog.sourcePathTbl.Text = _unrealService.ModuleRelativePath(sourcePath);

            if (string.IsNullOrEmpty(className))
            {
                _dialog.addButton.IsEnabled = false;
            }
            else
            {
                if (XFilesystem.FileExists(headerPath) || XFilesystem.FileExists(sourcePath))
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

        public Task ClassTypeChangedAsync()
        {
            string className = _dialog.classNameTbx.Text;
            int typeIndex = _dialog.GetClassTypeIndex();
            FUnrealSourceType sourceType = (FUnrealSourceType)typeIndex;

            _unrealService.ComputeSourceCodePaths(_absPathSelected, className, sourceType, out string headerPath, out string sourcePath, out _);

            _dialog.headerPathTbl.Text = _unrealService.ModuleRelativePath(headerPath);
            _dialog.sourcePathTbl.Text = _unrealService.ModuleRelativePath(sourcePath);

            if (XFilesystem.FileExists(headerPath) || XFilesystem.FileExists(sourcePath))
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

            string templeName = _templates[_dialog.classTemplCbx.SelectedIndex].Name;
            string absBasePath = _absPathSelected;
            string className = _dialog.classNameTbx.Text;
            int classType = _dialog.GetClassTypeIndex();

            FUnrealSourceType sourceType = (FUnrealSourceType)classType;

            var success = await _unrealService.AddSourceClassAsync(templeName, absBasePath, className, sourceType, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }

            _unrealVS.WhenProjectReload_MarkItemForSelection = success.HeaderPath;

            _dialog.Close();
        }
    }
}
