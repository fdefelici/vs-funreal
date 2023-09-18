using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FUnreal
{
    /// <summary>
    /// Interaction logic for GeneralOptions.xaml
    /// </summary>
    public partial class GeneralOptions : UserControl
    {
        internal CustomOptionPage generalOptionsPage;

        public GeneralOptions()
        {
            InitializeComponent();
        }

        public void Initialize()
        {

            cbMyOption.IsChecked = MyOptions.Instance.MyBool;
            //General.Instance.Save();
        }

        private void cbMyOption_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            MyOptions.Instance.MyBool = (bool)cbMyOption.IsChecked;
            MyOptions.Instance.Save();
        }

        private void cbMyOption_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            MyOptions.Instance.MyBool = (bool)cbMyOption.IsChecked;
            MyOptions.Instance.Save();
        }
    }
}
