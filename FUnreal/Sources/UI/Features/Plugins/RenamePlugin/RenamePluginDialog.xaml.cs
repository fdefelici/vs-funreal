using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;

namespace FUnreal
{
    public partial class RenamePluginDialog : DialogWindow
    {
        public const string ErrorMsg_PluginAlreadtExists = "A plugin already exists with thie name!";
        public const string ErrorMsg_SomthingWentWrong = "Ops! Something went wrong...";

        public Func<Task> OnConfirmAsync { get; set; }
        public Func<Task> OnRenameAsync { get; set; }
        public RenamePluginDialog()
        {
            InitializeComponent();

            pluginNewNameTbx.Text = "";
            pluginNewNameTbx.IsEnabled = true;
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

        private void pluginNewNameTbx_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OnRenameAsync?.Invoke().FireAndForget();
        }

        private void pluginNewNameTbx_Validation(object sender, System.Windows.Input.TextCompositionEventArgs e)
            => XDialogLib.TextBox_FileName_InputValidation(sender, e);

        public void EditModeEnabled(bool enabled)
        {
            pluginNewNameTbx.IsEnabled = enabled;
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
