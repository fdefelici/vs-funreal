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

        private void messageExpand_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsExpanded = !IsExpanded;
        }
    }
}
