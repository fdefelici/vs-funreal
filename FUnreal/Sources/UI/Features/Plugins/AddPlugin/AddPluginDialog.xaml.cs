using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public partial class AddPluginDialog : DialogWindow
    {
        public Func<Task> OnConfirmAsync { get; set; }
        public Func<Task> OnTemplateChangeAsync { get; set; }
        public Func<Task> OnPluginNameChangeAsync { get; set; }
        public Func<Task> OnModuleNameChangeAsync { get; set; }
        
        public AddPluginDialog()
            : base()
        {
            InitializeComponent();

            pluginTemplCbx.IsEnabled = true;
            pluginNameTbx.IsEnabled = true;
            moduleNameTbx.IsEnabled = true;
            addButton.IsEnabled = false;
            cancelButton.IsEnabled = true;
            HideError();

            taskProgressPanel.IsRendered = false;
            taskProgressPanel.IsExpanded = false;
        }

        private void okButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OnConfirmAsync?.Invoke().FireAndForget();
        }
        private void pluginTemplCbxChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            OnTemplateChangeAsync?.Invoke().FireAndForget();
        }

        private void pluginNameTbxChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OnPluginNameChangeAsync?.Invoke().FireAndForget();
        }

        private void moduleNameTbxChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OnModuleNameChangeAsync?.Invoke().FireAndForget();
        }

        private void inputText_Validation(object sender, System.Windows.Input.TextCompositionEventArgs e) 
            => XDialogLib.TextBox_FileName_InputValidation(sender, e);
       
       
        public void HideError()
        {
            errorMsgLbl.Content = "";
            errorMsgLbl.Visibility = System.Windows.Visibility.Hidden;
        }

        public void ShowModuleNameControls(bool show)
        {
            moduleNameTbx.Visibility = show ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            moduleRow1.Height = show ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            moduleRow2.Height = show ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
        }

        public void EditModeEnabled(bool enabled)
        {
            pluginTemplCbx.IsEnabled = enabled;
            pluginNameTbx.IsEnabled = enabled;
            moduleNameTbx.IsEnabled = enabled;
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

        public void ShowError(string msg, params string[] args)
        {
            errorMsgLbl.Content = XString.Format(msg, args);
            errorMsgLbl.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
