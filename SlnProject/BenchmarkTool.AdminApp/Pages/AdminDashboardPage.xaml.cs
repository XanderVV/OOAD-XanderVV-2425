using System;
using System.Windows;
using System.Windows.Controls;

namespace BenchmarkTool.AdminApp.Pages
{
    /// <summary>
    /// Interaction logic for AdminDashboardPage.xaml
    /// </summary>
    public partial class AdminDashboardPage : Page
    {
        public AdminDashboardPage()
        {
            InitializeComponent();
        }

        private void BtnBedrijvenBeheer_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as AdminMainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigeerNaarBedrijvenBeheer();
            }
            else
            {
                // Fallback naar de oude methode
                NavigationService.Navigate(new BedrijvenBeheerPage());
            }
        }

        private void BtnRegistratieBeheer_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as AdminMainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigeerNaarRegistratieBeheer();
            }
            else
            {
                // Fallback naar de oude methode
                NavigationService.Navigate(new RegistratieBeheerPage());
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as AdminMainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigeerNaarLogin();
            }
            else
            {
                // Fallback naar de oude methode
                NavigationService.Navigate(new AdminLoginPage());
            }
        }
    }
} 