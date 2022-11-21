using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace FUnreal
{
    public partial class AddFolderDialog : DialogWindow
    {
        public static AddFolderDialog CreateInRenameMode()
        {
            var view = new AddFolderDialog();
            view.Title = "FUnreal Toolbox: Rename Folder";
            view.addButton.Content = "Rename";
            view.pathLbl.Content = "Folder Name";
            view.OnInputValidation = XDialogLib.TextBox_FolderName_InputValidation;
            view.OnPasteValidation = XDialogLib.TextBox_FolderName_PasteValidation;
            return view;
        }

        public Func<Task> OnConfirmAsync { get; set; }
        public Func<Task> OnPathChangeAsync { get; set; }
        private Action<object, TextCompositionEventArgs> OnInputValidation { get; set; }
        private Action<object, DataObjectPastingEventArgs> OnPasteValidation { get; set; }

        public AddFolderDialog()
            : base()
        {
            InitializeComponent();

            pathTbx.IsEnabled = true;

            addButton.IsEnabled = false;
            cancelButton.IsEnabled = true;
            HideError();

            taskProgressPanel.IsRendered = false;
            taskProgressPanel.IsExpanded = false;

            OnInputValidation = XDialogLib.TextBox_SubPath_InputValidation;
            OnPasteValidation = XDialogLib.TextBox_SubPath_PasteValidation;
            
        }

        private void okButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OnConfirmAsync?.Invoke().FireAndForget();
        }

        private void classNameTbxChanged(object sender, TextChangedEventArgs e)
        {
            OnPathChangeAsync?.Invoke().FireAndForget();
        }

        private void inputText_Validation(object sender, TextCompositionEventArgs e)
        {
            OnInputValidation?.Invoke(sender, e);
        }
        private void pasteText_Validation(object sender, DataObjectPastingEventArgs e)
        {
           OnPasteValidation?.Invoke(sender, e);
        }


        public void HideError()
        {
            errorMsgLbl.Content = "";
            errorMsgLbl.Visibility = System.Windows.Visibility.Hidden;
        }

        public void EditModeEnabled(bool enabled)
        {
            pathTbx.IsEnabled = enabled;
            addButton.IsEnabled = enabled;
            cancelButton.IsEnabled = enabled;
        }

        public void ShowActionInProgress()
        {
            EditModeEnabled(false);

            taskProgressPanel.IsRendered = true;
            taskProgressPanel.IsExpanded = false;
        }

        public void ShowActionInError()
        {

            EditModeEnabled(true);

            ShowError(XDialogLib.ErrorMsg_SomethingWentWrong);

            taskProgressPanel.IsRendered = true;
            taskProgressPanel.IsExpanded = true;
        }

        public void SetProgressMessage(FUnrealNotifier.MessageType Type, string headMessage, string traceMessage)
        {
            if (Type == FUnrealNotifier.MessageType.ERRO) taskProgressPanel.SetFailureMode();
            else taskProgressPanel.SetProgressMode();

            string prefix = $"[{Type}]";
            string trace = $"{prefix} {traceMessage}";
            taskProgressPanel.AddMessage(headMessage, trace);
        }

        public void ShowError(string msg)
        {
            errorMsgLbl.Content = msg;
            errorMsgLbl.Visibility = System.Windows.Visibility.Visible;
        }

        public void ShowErrorFatal(string msg, params string[] args)
        {
            string errorMsg = XString.Format(msg, args);
            ShowError(errorMsg);

            EditModeEnabled(false);
            cancelButton.IsEnabled = true;
        }
    }
}
