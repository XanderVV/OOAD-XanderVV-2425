using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using BenchmarkTool.ClassLibrary.Authentication;

namespace BenchmarkTool.AdminApp.Pages
{
    /// <summary>
    /// Interaction logic for AdminLoginPage.xaml
    /// </summary>
    public partial class AdminLoginPage : Page
    {
        private readonly AuthenticatieService _authService;

        public AdminLoginPage()
        {
            InitializeComponent();
            _authService = new AuthenticatieService();

            // Zet het standaard "admin" wachtwoord vast in voor gemak
            txtUsername.Text = "admin";
            pwdPassword.Password = "admin"; // Automatisch invoeren voor snelle login

            // Debug: Direct testen bij het opstarten
            TestInloggegevens();
        }

        private void TestInloggegevens()
        {
            try
            {
                // Debug-informatie weergeven om het probleem te identificeren
                string configUsername = ConfigurationManager.AppSettings["AdminGebruikersnaam"];
                string configHash = ConfigurationManager.AppSettings["AdminWachtwoordHash"];

                if (string.IsNullOrEmpty(configUsername) || string.IsNullOrEmpty(configHash))
                {
                    txtError.Text = "Configuratie ontbreekt: gebruikersnaam of hash is leeg.";
                    return;
                }

                txtError.Text = "";

                // Test de configuratiewaarden
                Console.WriteLine($"Gebruikersnaam uit config: {configUsername}");
                Console.WriteLine($"Wachtwoordhash uit config: {configHash}");

                // Test met WachtwoordTestHelper
                string testWachtwoord = "admin";
                Console.WriteLine($"Testen van wachtwoord: '{testWachtwoord}'");
                WachtwoordTestHelper.TestWachtwoordVerificatie(configHash, testWachtwoord);

                // Test met de verbeterde veilige verificatie
                bool veiligeCheck = WachtwoordTestHelper.VeiligeVerificatie(configHash, testWachtwoord);
                Console.WriteLine($"Veilige verificatie resultaat: {veiligeCheck}");

                // Test validatie via de AuthenticatieService
                bool isValid = _authService.ValideerAdminCredentials("admin", "admin");
                Console.WriteLine($"Validatie via AuthenticatieService: {isValid}");
                
                if (isValid)
                {
                    Console.WriteLine("Authenticatie is succesvol! De login moet nu werken.");
                }
                else
                {
                    Console.WriteLine("Authenticatie mislukt - mogelijke oorzaken:");
                    Console.WriteLine("1. De hash format is gewijzigd (ASP.NET Core versies)");
                    Console.WriteLine("2. Onjuist wachtwoord (moet 'admin' zijn)");
                    Console.WriteLine("3. Probleem met PasswordHasher implementatie");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij testen van inloggegevens: {ex.Message}");
                txtError.Text = $"Fout bij testen: {ex.Message}";
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string gebruikersnaam = txtUsername.Text;
            string wachtwoord = pwdPassword.Password;

            // Valideer de invoer
            if (string.IsNullOrWhiteSpace(gebruikersnaam) || string.IsNullOrWhiteSpace(wachtwoord))
            {
                txtError.Text = "Vul alstublieft zowel gebruikersnaam als wachtwoord in.";
                return;
            }

            try
            {
                // Controleer admin credentials
                bool isValidAdmin = _authService.ValideerAdminCredentials(gebruikersnaam, wachtwoord);

                if (isValidAdmin)
                {
                    // Navigeer naar het dashboard
                    NavigationService.Navigate(new AdminDashboardPage());
                }
                else
                {
                    txtError.Text = "Ongeldige inloggegevens. Probeer het opnieuw.";
                }
            }
            catch (Exception ex)
            {
                txtError.Text = $"Er is een fout opgetreden: {ex.Message}";
            }
        }
    }
} 