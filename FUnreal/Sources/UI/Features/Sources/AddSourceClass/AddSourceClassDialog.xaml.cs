using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FUnreal
{
    public partial class AddSourceClassDialog : DialogWindow
    {
        private int _classTypeIndex;

        public Func<Task> OnConfirmAsync { get; set; }
        public Func<Task> OnTemplateChangeAsync { get; set; }
        public Func<Task> OnClassNameChangeAsync { get; set; }
        public Func<Task> OnClassTypeChangeAsync { get; set; }
        
        public AddSourceClassDialog()
            : base()
        {
            InitializeComponent();

            classTemplCbx.IsEnabled = true;
            classNameTbx.IsEnabled = true;
            publicRdb.IsEnabled = true;
            privateRdb.IsEnabled = true;
            freeRdb.IsEnabled = true;

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
        private void classTemplCbxChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            OnTemplateChangeAsync?.Invoke().FireAndForget();
        }

        private void classNameTbxChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OnClassNameChangeAsync?.Invoke().FireAndForget();
        }

        private void inputText_Validation(object sender, System.Windows.Input.TextCompositionEventArgs e) 
            => XDialogLib.TextBox_FileName_InputValidation(sender, e);


        private void publicRdb_Checked(object sender, RoutedEventArgs e)
        {
            _classTypeIndex = 0;
            OnClassTypeChangeAsync?.Invoke().FireAndForget();
        }

        private void privateRdb_Checked(object sender, RoutedEventArgs e)
        {
            _classTypeIndex = 1;
            OnClassTypeChangeAsync?.Invoke().FireAndForget();
        }

        private void freeRdb_Checked(object sender, RoutedEventArgs e)
        {
            _classTypeIndex = 2;
            OnClassTypeChangeAsync?.Invoke().FireAndForget();
        }


        public void SetClassTypeIndex(int index)
        {
            _classTypeIndex = index;
            if (_classTypeIndex == 0)
            {
                publicRdb.IsChecked = true;
            }
            else if (_classTypeIndex == 1)
            {
                privateRdb.IsChecked = true;
            }
            else if (_classTypeIndex == 2)
            {
                freeRdb.IsChecked = true;
            }
        }

        public int GetClassTypeIndex()
        {
            return _classTypeIndex;
        }

        public void HideError()
        {
            errorMsgLbl.Content = "";
            errorMsgLbl.Visibility = System.Windows.Visibility.Hidden;
        }
/*
        public void ShowModuleNameControls(bool show)
        {
            moduleNameTbx.Visibility = show ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            moduleRow1.Height = show ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            moduleRow2.Height = show ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
        }
*/

        public void EditModeEnabled(bool enabled)
        {
            classTemplCbx.IsEnabled = enabled;
            classNameTbx.IsEnabled = enabled;
            publicRdb.IsEnabled = enabled;
            privateRdb.IsEnabled = enabled;
            freeRdb.IsEnabled = enabled;
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
            string prefix = $"[{Type}]";
            string trace = $"{prefix} {traceMessage}";
            taskProgressPanel.AddMessage(headMessage, trace);
        }

        public void ShowError(string msg)
        {
            errorMsgLbl.Content = msg;
            errorMsgLbl.Visibility = System.Windows.Visibility.Visible;
        }

        public void ShowFreeCheckbox(bool visible)
        {
            freeRdb.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
