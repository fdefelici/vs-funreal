using Community.VisualStudio.Toolkit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public class AddSourceClassController : IXActionController
    {
        private AddSourceClassDialog _dialog;
        private FUnrealService _unrealService;
        private List<FUnrealTemplate> _templates;
        private FUnrealNotifier _notifier;
        private FUnrealVS _unrealVs;
        private string _absPathSelected;
        private FUnrealSourceType _absPathSelectedType;

        public AddSourceClassController(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            _unrealService = unrealService;
            _templates = _unrealService.SourceTemplates();
            _notifier = new FUnrealNotifier();
            _unrealVs = unrealVS;
        }

        public override bool ShouldBeVisible()
        {
            return _unrealVs.IsSingleSelectionAsync().GetAwaiter().GetResult();
        }

        public override async Task DoActionAsync()
        {
            _dialog = new AddSourceClassDialog();
            _dialog.OnTemplateChangeAsync = TemplateChangedAsync;
            _dialog.OnClassNameChangeAsync = ClassNameChangedAsync;
            _dialog.OnClassTypeChangeAsync = ClassTypeChangedAsync;
            _dialog.OnConfirmAsync = ConfirmAsync;
            _notifier.OnSendMessage = _dialog.SetProgressMessage;

            if (_templates.Count == 0)
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_TemplatesNotFound);
                return;
            }

            var item = await _unrealVs.GetSelectedItemAsync();
            _absPathSelected = item.FullPath;
            _absPathSelectedType = _unrealService.TypeForSourcePath(_absPathSelected);
            if (_absPathSelectedType == FUnrealSourceType.INVALID)
            {
                await VS.MessageBox.ShowErrorAsync(XDialogLib.ErrorMsg_InvalidPath, _absPathSelected);
                return;
            }

            //if type is Public or Private, Free type checkbox disappear beacuse is no sense
            if (_absPathSelectedType == FUnrealSourceType.PUBLIC || _absPathSelectedType == FUnrealSourceType.PRIVATE)
            {
                _dialog.ShowFreeCheckbox(false);
            }
            else
            {
                _dialog.ShowFreeCheckbox(true);
            }

           
            _dialog.classTemplCbx.ItemsSource = _templates;
            _dialog.classTemplCbx.SelectedIndex = 0;  //Fire Template Changed
            _dialog.classTemplTbl.Text = _templates[0].Description;

            await _dialog.ShowDialogAsync();
        }

        public Task TemplateChangedAsync()
        {
            int index = _dialog.classTemplCbx.SelectedIndex;
            FUnrealTemplate selected = _templates[index];
            _dialog.classTemplTbl.Text = selected.Description;

            /*
                _templateHasModuleName = selected.HasPlaceHolder("ModuleName");
                if (_templateHasModuleName)
                {
                    _dialog.ShowModuleNameControls(true);
                }
                else //Example: Content Only Plugin has no ModuleName
                {
                    _dialog.ShowModuleNameControls(false);
                }
            */

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

            _dialog.headerPathTbl.Text = _unrealService.RelPathToModule(headerPath);
            _dialog.sourcePathTbl.Text = _unrealService.RelPathToModule(sourcePath);

            if (string.IsNullOrEmpty(className))
            {
                _dialog.addButton.IsEnabled = false;
            }
            else
            {
                if (XFilesystem.FileExists(headerPath) || XFilesystem.FileExists(sourcePath))
                {
                    _dialog.addButton.IsEnabled = false;
                    _dialog.ShowError(XDialogLib.ErrorMsg_ClassAlreadyExists);
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

            _dialog.headerPathTbl.Text = _unrealService.RelPathToModule(headerPath);
            _dialog.sourcePathTbl.Text = _unrealService.RelPathToModule(sourcePath);
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

            bool success = await _unrealService.AddSourceAsync(templeName, absBasePath, className, sourceType, _notifier);
            if (!success)
            {
                _dialog.ShowActionInError();
                return;
            }
            _dialog.Close();
        }
    }
}
