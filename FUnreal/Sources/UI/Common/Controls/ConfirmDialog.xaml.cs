using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;


namespace FUnreal
{
    public partial class ConfirmDialog : DialogWindow
    {
        public Func<Task> OnConfirm { get; set; }
  
        public ConfirmDialog(string msg1, string msg2 = "")
            : base()
        {
            InitializeComponent();

            msg1Lbl.Content = msg1;
            msg2Lbl.Content = msg2;
            errorMsgLbl.Visibility = System.Windows.Visibility.Hidden;

            taskProgressPanel.IsRendered = false;
            taskProgressPanel.IsExpanded = false;
        }

        private void ConfirmBtnClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            OnConfirm?.Invoke().FireAndForget();
        }

        public void ShowActionInProgress()
        {
            confirmBtn.IsEnabled = false;
            cancelBtn.IsEnabled = false;

            taskProgressPanel.IsRendered = true;
            taskProgressPanel.IsExpanded = false;
        }

        public void SetProgressMessage(FUnrealNotifier.MessageType Type, string headMessage, string traceMessage)
        {
            string prefix = $"[{Type}]";
            string trace = $"{prefix} {traceMessage}";
            taskProgressPanel.AddMessage(headMessage, trace);
        }

        public void ShowActionInError()
        {
            confirmBtn.IsEnabled = false;
            cancelBtn.IsEnabled = true;
            errorMsgLbl.Content = XDialogLib.ErrorMsg_SomethingWentWrong;
            errorMsgLbl.Visibility = System.Windows.Visibility.Visible;
 
            taskProgressPanel.IsRendered = true;
            taskProgressPanel.IsExpanded = true;
        }
    }
}
