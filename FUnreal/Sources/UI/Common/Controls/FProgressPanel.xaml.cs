using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FUnreal
{
    /// <summary>
    /// Interaction logic for FProgressPanel.xaml
    /// </summary>
    public partial class FProgressPanel : UserControl
    {
        /*
        #region Properties
        https://stackoverflow.com/questions/30240274/how-to-expose-internal-controls-property-to-its-parent-usercontrol-in-wpf
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register
            (
                "Text",
                typeof(string),
                typeof(FProgressPanel),
                new FrameworkPropertyMetadata("")
            );

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        #endregion
        */

        private const string EXPANDED =  "[-]";
        private const string COLLAPSED = "[+]";

        public FProgressPanel()
        {
            InitializeComponent();

            logPanel.Visibility = Visibility.Collapsed;

            expanderTbl.Text = COLLAPSED;
            messageTbl.Text = "";
            logTbl.Text = "";
        }

        public bool IsRendered
        {
            get { return Visibility == Visibility.Visible; }
            set { Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool IsExpanded
        {
            get { return logPanel.Visibility == Visibility.Visible; }

            set 
            {
                logPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                expanderTbl.Text = value ? EXPANDED : COLLAPSED;
            }
        }

        public void AddMessage(string shortMessage, string logMessage)
        {
            messageTbl.Text = shortMessage;

            logMessage = logMessage.Replace("\r\n", "\n");
            string[] lines = logMessage.Split('\n');
            foreach(var eachLine in lines)
            {
                logTbl.Inlines.Add(new Run(eachLine));
                logTbl.Inlines.Add(new LineBreak());
            }
        }

        public void SetFailureMode()
        {
            if (progressBar.IsIndeterminate == false) return;

            progressBar.IsIndeterminate = false;
            progressBar.Minimum = 100;
            progressBar.Foreground = System.Windows.Media.Brushes.Red;
        }

        public void SetProgressMode()
        {
            if (progressBar.IsIndeterminate == true) return;

            progressBar.IsIndeterminate = true;
            progressBar.Minimum = 0;
            progressBar.Foreground = System.Windows.Media.Brushes.Green;
        }

        private void messageExpand_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsExpanded = !IsExpanded;
        }
    }
}
