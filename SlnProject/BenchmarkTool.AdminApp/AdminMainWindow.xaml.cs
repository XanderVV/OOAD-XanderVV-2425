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
using BenchmarkTool.AdminApp.Pages;

namespace BenchmarkTool.AdminApp
{
    /// <summary>
    /// Interaction logic for AdminMainWindow.xaml
    /// </summary>
    public partial class AdminMainWindow : Window
    {
        /// <summary>
        /// Gets the main frame for navigation, making it accessible for tests
        /// </summary>
        public Frame NavigationFrame => MainFrame;
        
        public AdminMainWindow()
        {
            InitializeComponent();
            
            // Navigeer naar de login pagina bij het opstarten
            NavigeerNaar(new Pages.AdminLoginPage());
        }

        /// <summary>
        /// Centraal navigatiepunt voor de applicatie
        /// </summary>
        /// <param name="pagina">De pagina waar naartoe genavigeerd moet worden</param>
        public void NavigeerNaar(Page pagina)
        {
            MainFrame.Navigate(pagina);
        }

        /// <summary>
        /// Navigeer naar het admin dashboard
        /// </summary>
        public void NavigeerNaarDashboard()
        {
            NavigeerNaar(new AdminDashboardPage());
        }

        /// <summary>
        /// Navigeer naar de bedrijvenbeheer pagina
        /// </summary>
        public void NavigeerNaarBedrijvenBeheer()
        {
            NavigeerNaar(new BedrijvenBeheerPage());
        }

        /// <summary>
        /// Navigeer naar de registratiebeheer pagina
        /// </summary>
        public void NavigeerNaarRegistratieBeheer()
        {
            NavigeerNaar(new RegistratieBeheerPage());
        }

        /// <summary>
        /// Navigeer terug naar de login pagina (uitloggen)
        /// </summary>
        public void NavigeerNaarLogin()
        {
            NavigeerNaar(new AdminLoginPage());
        }

        /// <summary>
        /// Testfunctie om alle navigatie te valideren - kan vanuit een testomgeving aangeroepen worden
        /// </summary>
        /// <returns>True als alles werkt</returns>
        public bool TestNavigatie()
        {
            try
            {
                // Test alle navigatiefuncties
                NavigeerNaarDashboard();
                NavigeerNaarBedrijvenBeheer();
                NavigeerNaarRegistratieBeheer();
                NavigeerNaarLogin();
                
                // Als we hier komen, werkt de navigatie
                return true;
            }
            catch (System.Exception)
            {
                // Er is iets fout gegaan met de navigatie
                return false;
            }
        }

        #region Menu Event Handlers

        private void MnuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigeerNaarLogin();
        }

        private void MnuAfsluiten_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MnuDashboard_Click(object sender, RoutedEventArgs e)
        {
            NavigeerNaarDashboard();
        }

        private void MnuBedrijvenBeheer_Click(object sender, RoutedEventArgs e)
        {
            NavigeerNaarBedrijvenBeheer();
        }

        private void MnuRegistratieBeheer_Click(object sender, RoutedEventArgs e)
        {
            NavigeerNaarRegistratieBeheer();
        }

        #endregion
    }
}