using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BenchmarkTool.ClassLibrary.Authentication;
using BenchmarkTool.ClassLibrary.Models;
using BenchmarkTool.ClassLibrary.Services;

namespace BenchmarkTool.CompanyApp.Pages
{
    /// <summary>
    /// Loginpagina voor bedrijfsgebruikers
    /// </summary>
    public partial class CompanyLoginPage : Page
    {
        // Referentie naar het hoofdvenster
        private CompanyMainWindow _hoofdVenster;
        private readonly AuthenticatieService _authenticatieService;
        private readonly BedrijfService _bedrijfService;

        public CompanyLoginPage()
        {
            InitializeComponent();
            txtUsername.Focus();
            
            // Haal het hoofdvenster op
            _hoofdVenster = (CompanyMainWindow)Application.Current.MainWindow;
            
            // Initialiseer services
            _authenticatieService = new AuthenticatieService();
            _bedrijfService = new BedrijfService();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void pwdPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login();
            }
        }

        private void btnRegistreer_Click(object sender, RoutedEventArgs e)
        {
            _hoofdVenster.NavigeerNaar(new RegistratiePage());
        }

        private async void Login()
        {
            string sGebruikersnaam = txtUsername.Text.Trim();
            string sWachtwoord = pwdPassword.Password;

            // Validatie
            if (string.IsNullOrEmpty(sGebruikersnaam) || string.IsNullOrEmpty(sWachtwoord))
            {
                txtError.Text = "Voer uw gebruikersnaam en wachtwoord in.";
                return;
            }

            try
            {
                txtError.Text = "";
                prgLogin.Visibility = Visibility.Visible;
                btnLogin.IsEnabled = false;

                // Implementeer de authenticatie met de ClassLibrary
                Bedrijf bedrijf = _authenticatieService.ValideerBedrijfsCredentials(sGebruikersnaam, sWachtwoord);

                // Simuleer een delay om asynchrone operatie te demonstreren
                await System.Threading.Tasks.Task.Delay(500);

                if (bedrijf != null)
                {
                    // Haal logo op als het nog niet in het bedrijfsobject zit
                    if (bedrijf.Logo == null || bedrijf.Logo.Length == 0)
                    {
                        bedrijf.Logo = _bedrijfService.GetCompanyLogo(bedrijf.Id);
                    }

                    // Sla het ingelogde bedrijf op in een static property zodat andere pagina's het kunnen gebruiken
                    App.IngelogdBedrijf = bedrijf;

                    // Stel bedrijfsgegevens in op het hoofdvenster
                    _hoofdVenster.SetBedrijfsgegevens(bedrijf.Name, bedrijf.Logo);
                    _hoofdVenster.NavigeerNaar(new CompanyDashboardPage());
                }
                else
                {
                    txtError.Text = "Ongeldige gebruikersnaam of wachtwoord.";
                }
            }
            catch (Exception ex)
            {
                txtError.Text = $"Fout bij inloggen: {ex.Message}";
            }
            finally
            {
                prgLogin.Visibility = Visibility.Collapsed;
                btnLogin.IsEnabled = true;
            }
        }
    }
} 