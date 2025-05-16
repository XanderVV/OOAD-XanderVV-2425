using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Microsoft.Win32;
using BenchmarkTool.ClassLibrary.Data;
using BenchmarkTool.ClassLibrary.Models;

namespace BenchmarkTool.AdminApp.Pages
{
    /// <summary>
    /// Interaction logic for BedrijfFormulierPage.xaml
    /// </summary>
    public partial class BedrijfFormulierPage : Page
    {
        private readonly BedrijfRepository _bedrijfRepository;
        private readonly DatabaseRepository _databaseRepository;
        private Bedrijf _bedrijf;
        private bool _isEditMode;
        private byte[] _logoData;

        /// <summary>
        /// Constructor voor het aanmaken van een nieuw bedrijf
        /// </summary>
        public BedrijfFormulierPage()
        {
            InitializeComponent();
            _bedrijfRepository = new BedrijfRepository();
            _databaseRepository = new DatabaseRepository();
            _isEditMode = false;
            _bedrijf = new Bedrijf
            {
                Id = 0, // Zorg dat de Id expliciet op 0 wordt gezet, niet null
                RegDate = DateTime.Now,
                Status = "Pending",
                Language = "Nederlands"
            };

            // Stel de status standaard in op 'Actief'
            cboStatus.SelectedIndex = 0;
            
            // Stel de taal standaard in op 'Nederlands'
            cboLanguage.SelectedIndex = 0;
            
            // Laad NACE-codes
            LaadNacecodes();
        }

        /// <summary>
        /// Constructor voor het bewerken van een bestaand bedrijf
        /// </summary>
        /// <param name="bedrijfId">ID van het te bewerken bedrijf</param>
        public BedrijfFormulierPage(int bedrijfId)
        {
            InitializeComponent();
            _bedrijfRepository = new BedrijfRepository();
            _databaseRepository = new DatabaseRepository();
            _isEditMode = true;
            
            // Laad het bedrijf
            _bedrijf = _bedrijfRepository.GetById(bedrijfId);
            
            if (_bedrijf == null)
            {
                MessageBox.Show("Bedrijf niet gevonden.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.GoBack();
                return;
            }
            
            // Update de header tekst
            txtHeader.Text = "Bedrijf Bewerken";
            
            // Vul de formuliervelden met de bedrijfsgegevens
            VulFormulier();
            
            // Laad NACE-codes
            LaadNacecodes();
            
            // Laad het logo indien aanwezig
            LaadLogo();
        }

        private void LaadNacecodes()
        {
            try
            {
                // Haal alle NACE-codes op
                var naceCodes = _databaseRepository.GetAllNacecodes();
                cboNacecode.ItemsSource = naceCodes;
                
                // Als we in edit mode zijn, selecteer de juiste NACE-code
                if (_isEditMode && !string.IsNullOrEmpty(_bedrijf.NacecodeCode))
                {
                    foreach (var naceCode in naceCodes)
                    {
                        if (naceCode.Code == _bedrijf.NacecodeCode)
                        {
                            cboNacecode.SelectedItem = naceCode;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij ophalen van NACE-codes: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VulFormulier()
        {
            // Vul de bedrijfsgegevens
            txtName.Text = _bedrijf.Name;
            txtContact.Text = _bedrijf.Contact;
            txtEmail.Text = _bedrijf.Email;
            txtAddress.Text = _bedrijf.Address;
            txtZip.Text = _bedrijf.Zip;
            txtCity.Text = _bedrijf.City;
            txtCountry.Text = _bedrijf.Country;
            txtPhone.Text = _bedrijf.Phone;
            txtLogin.Text = _bedrijf.Login;
            
            // Selecteer de juiste status
            foreach (ComboBoxItem item in cboStatus.Items)
            {
                if (item.Tag.ToString() == _bedrijf.Status)
                {
                    cboStatus.SelectedItem = item;
                    break;
                }
            }
            
            // Selecteer de juiste taal
            foreach (ComboBoxItem item in cboLanguage.Items)
            {
                if (item.Tag.ToString() == _bedrijf.Language)
                {
                    cboLanguage.SelectedItem = item;
                    break;
                }
            }
            
            // Verberg wachtwoordveld bij bewerken (wachtwoord wordt niet gewijzigd tenzij expliciet ingevuld)
            lblPassword.Text = "Wachtwoord (alleen invullen om te wijzigen):";
        }

        private void LaadLogo()
        {
            try
            {
                // Haal het logo op uit de database
                _logoData = _bedrijfRepository.GetLogo(_bedrijf.Id);
                
                if (_logoData != null && _logoData.Length > 0)
                {
                    // Converteer de bytes naar een afbeelding
                    var ms = new MemoryStream(_logoData);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    
                    // Toon het logo
                    imgLogo.Source = image;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij ophalen van logo: {ex.Message}", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool ValideerFormulier()
        {
            // Verplichte velden
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Naam is verplicht.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Gebruikersnaam is verplicht.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLogin.Focus();
                return false;
            }
            
            // Wachtwoord is verplicht bij het aanmaken van een nieuw bedrijf
            if (!_isEditMode && string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Wachtwoord is verplicht.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
                return false;
            }
            
            // Controleer of gebruikersnaam al bestaat (bij nieuw bedrijf of gewijzigde gebruikersnaam)
            if ((!_isEditMode || (_isEditMode && txtLogin.Text != _bedrijf.Login)) && 
                _bedrijfRepository.GetBedrijfByLogin(txtLogin.Text) != null)
            {
                MessageBox.Show("Deze gebruikersnaam is al in gebruik. Kies een andere gebruikersnaam.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLogin.Focus();
                return false;
            }
            
            return true;
        }

        private void btnUploadLogo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open bestandskiezer voor afbeeldingen
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Selecteer een logo",
                    Filter = "Afbeeldingsbestanden (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                    Multiselect = false
                };
                
                if (openFileDialog.ShowDialog() == true)
                {
                    // Lees het bestand
                    _logoData = File.ReadAllBytes(openFileDialog.FileName);
                    
                    // Toon het geselecteerde logo
                    var ms = new MemoryStream(_logoData);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    
                    imgLogo.Source = image;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij uploaden van logo: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnVerwijderLogo_Click(object sender, RoutedEventArgs e)
        {
            // Reset logo
            _logoData = null;
            imgLogo.Source = null;
        }

        private void btnOpslaan_Click(object sender, RoutedEventArgs e)
        {
            if (!ValideerFormulier())
                return;
            
            try
            {
                // Debug logging voor nace code
                var selectedNacecode = cboNacecode.SelectedItem as Nacecode;
                System.Diagnostics.Debug.WriteLine($"[OPSLAAN] Geselecteerde NACE-code: {(selectedNacecode != null ? selectedNacecode.Code : "NULL")}");
                
                // Zet form data naar bedrijfsobject
                _bedrijf.Name = txtName.Text;
                _bedrijf.Contact = txtContact.Text;
                _bedrijf.Email = txtEmail.Text;
                _bedrijf.Address = txtAddress.Text;
                _bedrijf.Zip = txtZip.Text;
                _bedrijf.City = txtCity.Text;
                _bedrijf.Country = txtCountry.Text;
                _bedrijf.Phone = txtPhone.Text;
                _bedrijf.Login = txtLogin.Text;
                
                // Debug logging voor status
                System.Diagnostics.Debug.WriteLine($"[OPSLAAN] Status ComboBox SelectedItem: {cboStatus.SelectedItem}");
                
                // Status
                if (cboStatus.SelectedItem is ComboBoxItem statusItem)
                {
                    _bedrijf.Status = statusItem.Tag.ToString();
                    System.Diagnostics.Debug.WriteLine($"[OPSLAAN] Status gezet naar: {_bedrijf.Status}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[OPSLAAN] Geen status geselecteerd!");
                }
                
                // Debug logging voor taal
                System.Diagnostics.Debug.WriteLine($"[OPSLAAN] Taal ComboBox SelectedItem: {cboLanguage.SelectedItem}");
                
                // Taal
                if (cboLanguage.SelectedItem is ComboBoxItem languageItem)
                {
                    _bedrijf.Language = languageItem.Tag.ToString();
                    System.Diagnostics.Debug.WriteLine($"[OPSLAAN] Taal gezet naar: {_bedrijf.Language}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[OPSLAAN] Geen taal geselecteerd!");
                }
                
                // NACE-code
                if (selectedNacecode != null)
                {
                    _bedrijf.NacecodeCode = selectedNacecode.Code;
                    System.Diagnostics.Debug.WriteLine($"[OPSLAAN] NACE-code gezet naar: {_bedrijf.NacecodeCode}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[OPSLAAN] Geen NACE-code geselecteerd!");
                }
                
                // Stel registratiedatum in bij nieuw bedrijf
                if (!_isEditMode)
                {
                    _bedrijf.RegDate = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine($"[OPSLAAN] Registratiedatum gezet naar: {_bedrijf.RegDate}");
                }
                
                // Update lastmodified datum
                _bedrijf.LastModified = DateTime.Now;
                
                // Debug logging voor belangrijke velden
                System.Diagnostics.Debug.WriteLine($"[OPSLAAN] Naam: {_bedrijf.Name}, Login: {_bedrijf.Login}");
                System.Diagnostics.Debug.WriteLine($"[OPSLAAN] Status: {_bedrijf.Status}, Taal: {_bedrijf.Language}");
                System.Diagnostics.Debug.WriteLine($"[OPSLAAN] NACE-code: {_bedrijf.NacecodeCode}");
                System.Diagnostics.Debug.WriteLine($"[OPSLAAN] Wachtwoord ingevuld: {!string.IsNullOrEmpty(txtPassword.Password)}");
                
                bool success;
                
                if (_isEditMode)
                {
                    // Update bestaand bedrijf
                    success = _bedrijfRepository.Update(_bedrijf);
                    
                    // Update wachtwoord indien ingevuld
                    if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                    {
                        _bedrijfRepository.UpdatePassword(_bedrijf.Id, txtPassword.Password);
                    }
                }
                else
                {
                    // Voeg nieuw bedrijf toe
                    System.Diagnostics.Debug.WriteLine("[OPSLAAN] Start toevoegen van nieuw bedrijf");
                    _bedrijf.Id = _bedrijfRepository.Add(_bedrijf, txtPassword.Password);
                    System.Diagnostics.Debug.WriteLine($"[OPSLAAN] Bedrijf toegevoegd met ID: {_bedrijf.Id}");
                    success = _bedrijf.Id > 0;
                }
                
                // Update logo indien gewijzigd
                if (success && _logoData != null)
                {
                    _bedrijfRepository.UpdateLogo(_bedrijf.Id, _logoData);
                }
                else if (success && _logoData == null && _isEditMode)
                {
                    // Verwijder logo indien nodig
                    _bedrijfRepository.DeleteLogo(_bedrijf.Id);
                }
                
                if (success)
                {
                    MessageBox.Show(_isEditMode ? "Bedrijf is bijgewerkt." : "Bedrijf is toegevoegd.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Ga terug naar bedrijvenbeheer
                    NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show(_isEditMode ? "Kon het bedrijf niet bijwerken." : "Kon het bedrijf niet toevoegen.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Log de fout voor debugging
                System.Diagnostics.Debug.WriteLine($"Fout bij opslaan bedrijf: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    MessageBox.Show($"Fout bij opslaan: {ex.Message}\nDetails: {ex.InnerException?.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Fout bij opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAnnuleren_Click(object sender, RoutedEventArgs e)
        {
            // Vraag om bevestiging als er wijzigingen zijn gemaakt
            MessageBoxResult result = MessageBox.Show("Weet u zeker dat u wilt annuleren? Eventuele wijzigingen gaan verloren.", 
                "Annuleren", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                NavigationService.GoBack();
            }
        }

        private void btnTerug_Click(object sender, RoutedEventArgs e)
        {
            // Zelfde functionaliteit als annuleren
            btnAnnuleren_Click(sender, e);
        }
    }
} 