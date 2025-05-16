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

        private void btnBedrijvenBeheer_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new BedrijvenBeheerPage());
        }

        private void btnRegistratieBeheer_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegistratieBeheerPage());
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Ga terug naar de login pagina
            NavigationService.Navigate(new AdminLoginPage());
        }
    }
} 