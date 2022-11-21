using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public partial class RenameGameModuleDialog : DialogWindow
    {
        public Func<Task> OnConfirmAsync { get; set; }
        public Func<Task> OnRenameAsync { get; set; }

        public RenameGameModuleDialog()
        {
            InitializeComponent();

            renameFilesCbx.IsEnabled = true;
            confirmBtn.IsEnabled = true;
            cancelBtn.IsEnabled = true;
            HideError();

            taskProgressPanel.IsRendered = false;
            taskProgressPanel.IsExpanded = false;
        }

        public void HideError()
        {
            errorMsgLbl.Content = "";
            errorMsgLbl.Visibility = System.Windows.Visibility.Hidden;
        }

        private void confirmBtn_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            OnConfirmAsync?.Invoke().FireAndForget();
        }

        private void moduleNewNameTbx_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OnRenameAsync?.Invoke().FireAndForget();
        }

        private void inputText_Validation(object sender, System.Windows.Input.TextCompositionEventArgs e)
            => XDialogLib.TextBox_ClassName_InputValidation(sender, e);

        private void pasteText_Validation(object sender, DataObjectPastingEventArgs e)
            => XDialogLib.TextBox_ClassName_PasteValidation(sender, e);

        private void renameFilesCbx_Changed(object sender, RoutedEventArgs e)
        {
            //nothing todo 
        }


        public void EditModeEnabled(bool enabled)
        {
            moduleNewNameTbx.IsEnabled = enabled;
            confirmBtn.IsEnabled = enabled;
            cancelBtn.IsEnabled = enabled;
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


    }
}
