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

namespace Watson.SecretGenerate
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            SecretConnect.Text = string.Empty;
            SecretIV.Text = string.Empty;
            ConnectEncrypt encrypt = new ConnectEncrypt(SecretKey.Text, IsRandomIV.IsChecked.Value);
            SecretConnect.Text = encrypt.SecretCode(ConnectionString.Text);
            if (IsRandomIV.IsChecked.Value)
                SecretIV.Text = encrypt.SecretIV();
        }
    }
}
