using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;


namespace FUnreal
{
    public partial class MessageDialog : DialogWindow
    {
        public Func<Task> OnConfirm { get; set; }
  
        public MessageDialog(string msg1, string msg2 = "")
            : base()
        {
            InitializeComponent();

            msg1Lbl.Content = msg1;
            msg2Lbl.Text = msg2;
        }

        private void ConfirmBtnClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            OnConfirm?.Invoke().FireAndForget();
        }

    }
}
