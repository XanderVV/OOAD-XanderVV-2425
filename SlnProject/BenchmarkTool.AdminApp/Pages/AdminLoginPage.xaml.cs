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
            
            // Focus op het gebruikersnaamveld
            Loaded += (s, e) => { txtUsername.Focus(); };
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
                // UI-elementen bijwerken voor login
                btnLogin.IsEnabled = false;
                txtUsername.IsEnabled = false;
                pwdPassword.IsEnabled = false;
                prgLogin.Visibility = Visibility.Visible;
                txtError.Text = "Bezig met inloggen...";
                
                // Controleer admin credentials
                bool isValidAdmin = _authService.ValideerAdminCredentials(gebruikersnaam, wachtwoord);

                if (isValidAdmin)
                {
                    // Wis eventuele foutmeldingen
                    txtError.Text = string.Empty;
                    
                    // Gebruik de centraal gedefinieerde navigatiemethode in het hoofdvenster
                    var mainWindow = (Window.GetWindow(this) as AdminMainWindow);
                    if (mainWindow != null)
                    {
                        mainWindow.NavigeerNaarDashboard();
                    }
                    else
                    {
                        // Fallback naar de oude methode als het hoofdvenster niet beschikbaar is
                        NavigationService.Navigate(new AdminDashboardPage());
                    }
                }
                else
                {
                    // Reset UI-elementen
                    btnLogin.IsEnabled = true;
                    txtUsername.IsEnabled = true;
                    pwdPassword.IsEnabled = true;
                    prgLogin.Visibility = Visibility.Collapsed;
                    
                    // Toon foutmelding
                    txtError.Text = "Ongeldige inloggegevens. Probeer het opnieuw.";
                    pwdPassword.Password = string.Empty;
                    pwdPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                // Reset UI-elementen
                btnLogin.IsEnabled = true;
                txtUsername.IsEnabled = true;
                pwdPassword.IsEnabled = true;
                prgLogin.Visibility = Visibility.Collapsed;
                
                // Toon foutmelding
                txtError.Text = $"Er is een fout opgetreden: {ex.Message}";
            }
        }
        
        private void pwdPassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }
    }
} 