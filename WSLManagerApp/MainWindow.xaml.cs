// Fichier : MainWindow.xaml.cs
// Auteur : Le Geek Zen
// Description : Code-behind de la fenêtre principale. Gère la navigation entre les différentes pages de l'application.
//
// Emplacement : WSLManagerApp/MainWindow.xaml.cs

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WSLManagerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            MainFrame.Content = new Pages.SystemInfoPage();
        }

        private void BtnSystemInfo_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new Pages.SystemInfoPage();
        }

        private void BtnInstallDistro_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new Pages.InstallDistroPage();
        }

        private void BtnManageDistros_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new Pages.ManageDistrosPage();
        }
    }
}