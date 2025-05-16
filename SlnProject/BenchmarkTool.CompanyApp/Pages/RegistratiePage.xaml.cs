using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using BenchmarkTool.ClassLibrary.Models;
using BenchmarkTool.ClassLibrary.Services;

namespace BenchmarkTool.CompanyApp.Pages
{
    /// <summary>
    /// Registratiepagina voor nieuwe bedrijven
    /// </summary>
    public partial class RegistratiePage : Page
    {
        private byte[] _logoData = null;
        // Referentie naar het hoofdvenster
        private CompanyMainWindow _hoofdVenster;
        private readonly BedrijfService _bedrijfService;

        public RegistratiePage()
        {
            InitializeComponent();
            // Haal het hoofdvenster op
            _hoofdVenster = (CompanyMainWindow)Application.Current.MainWindow;
            // Initialiseer service
            _bedrijfService = new BedrijfService();
        }

        private void btnTerug_Click(object sender, RoutedEventArgs e)
        {
            _hoofdVenster.NavigeerNaar(new CompanyLoginPage());
        }

        private void btnKiesLogo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Kies een bedrijfslogo",
                Filter = "Afbeeldingsbestanden (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _logoData = File.ReadAllBytes(openFileDialog.FileName);
                    txtLogoPath.Text = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fout bij het lezen van het logobestand: " + ex.Message, 
                                   "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void btnRegistreer_Click(object sender, RoutedEventArgs e)
        {
            // Validatie
            if (!ValideerFormulier())
            {
                return;
            }

            try
            {
                // UI bijwerken
                prgRegistratie.Visibility = Visibility.Visible;
                btnRegistreer.IsEnabled = false;
                txtStatusMessage.Text = "Bezig met registreren...";

                // Maak een nieuw bedrijfsobject met de ingevulde gegevens
                Bedrijf nieuwBedrijf = new Bedrijf
                {
                    Name = txtNaam.Text.Trim(),
                    Contact = txtContact.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Phone = txtTelefoon.Text.Trim(),
                    Btw = txtBTW.Text.Trim(),
                    Login = txtLogin.Text.Trim(),
                    Address = txtAdres.Text.Trim(),
                    Zip = txtPostcode.Text.Trim(),
                    City = txtPlaats.Text.Trim(),
                    Country = txtLand.Text.Trim(),
                    Language = (cmbTaal.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Logo = _logoData,
                    RegDate = DateTime.Now,
                    Status = "Pending"
                };

                // Registreer het nieuwe bedrijf via de service
                int resultaat = _bedrijfService.CreateCompany(nieuwBedrijf);

                // Simuleer een vertraging om asynchrone operatie te demonstreren
                await System.Threading.Tasks.Task.Delay(1000);

                if (resultaat > 0)
                {
                    // Toon succes bericht
                    txtStatusMessage.Text = "Registratie succesvol verzonden! U ontvangt een e-mail na goedkeuring door de administrator.";
                    
                    // Automatisch terug naar login na 3 seconden
                    await System.Threading.Tasks.Task.Delay(3000);
                    _hoofdVenster.NavigeerNaar(new CompanyLoginPage());
                }
                else
                {
                    txtStatusMessage.Text = "Fout bij registratie: kon bedrijf niet opslaan.";
                }
            }
            catch (Exception ex)
            {
                txtStatusMessage.Text = "Fout bij registratie: " + ex.Message;
            }
            finally
            {
                prgRegistratie.Visibility = Visibility.Collapsed;
                btnRegistreer.IsEnabled = true;
            }
        }

        private bool ValideerFormulier()
        {
            // Reset statusbericht
            txtStatusMessage.Text = "";

            // Controleer verplichte velden
            if (string.IsNullOrWhiteSpace(txtNaam.Text))
            {
                txtStatusMessage.Text = "Bedrijfsnaam is verplicht.";
                txtNaam.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtContact.Text))
            {
                txtStatusMessage.Text = "Contactpersoon is verplicht.";
                txtContact.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtStatusMessage.Text = "E-mailadres is verplicht.";
                txtEmail.Focus();
                return false;
            }
            else if (!IsGeldigEmailadres(txtEmail.Text))
            {
                txtStatusMessage.Text = "Voer een geldig e-mailadres in.";
                txtEmail.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                txtStatusMessage.Text = "Login is verplicht.";
                txtLogin.Focus();
                return false;
            }

            // Alle validaties geslaagd
            return true;
        }

        private bool IsGeldigEmailadres(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
} 