using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BenchmarkTool.ClassLibrary.Data;
using BenchmarkTool.ClassLibrary.Models;
using Microsoft.Win32;

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
                
                // Maak de ComboBox leeg
                cboNacecode.Items.Clear();
                
                // Voeg elke NACE-code toe als ComboBoxItem
                foreach (var naceCode in naceCodes)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = naceCode.Code + " - " + naceCode.Text;
                    item.Tag = naceCode.Code;
                    cboNacecode.Items.Add(item);
                    
                    // Als we in edit mode zijn, selecteer de juiste NACE-code
                    if (_isEditMode && naceCode.Code == _bedrijf.NacecodeCode)
                    {
                        cboNacecode.SelectedItem = item;
                    }
                }
                
                // Als er geen items geselecteerd zijn en er zijn items beschikbaar, selecteer de eerste
                if (cboNacecode.SelectedIndex == -1 && cboNacecode.Items.Count > 0)
                {
                    cboNacecode.SelectedIndex = 0;
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

        private void BtnUploadLogo_Click(object sender, RoutedEventArgs e)
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

        private void BtnVerwijderLogo_Click(object sender, RoutedEventArgs e)
        {
            // Reset logo
            _logoData = null;
            imgLogo.Source = null;
        }

        private void BtnOpslaan_Click(object sender, RoutedEventArgs e)
        {
            if (!ValideerFormulier())
                return;
            
            try
            {
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
                
                // Status
                if (cboStatus.SelectedItem is ComboBoxItem statusItem)
                {
                    _bedrijf.Status = statusItem.Tag.ToString();
                }
                
                // Taal
                if (cboLanguage.SelectedItem is ComboBoxItem languageItem)
                {
                    _bedrijf.Language = languageItem.Tag.ToString();
                }
                
                // NACE-code
                if (cboNacecode.SelectedItem is ComboBoxItem nacecodeItem)
                {
                    _bedrijf.NacecodeCode = nacecodeItem.Tag.ToString();
                }
                
                // Stel registratiedatum in bij nieuw bedrijf
                if (!_isEditMode)
                {
                    _bedrijf.RegDate = DateTime.Now;
                }
                
                // Update lastmodified datum
                _bedrijf.LastModified = DateTime.Now;
                
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
                    _bedrijf.Id = _bedrijfRepository.Add(_bedrijf, txtPassword.Password);
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
                if (ex.InnerException != null)
                {
                    MessageBox.Show($"Fout bij opslaan: {ex.Message}\nDetails: {ex.InnerException?.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Fout bij opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnAnnuleren_Click(object sender, RoutedEventArgs e)
        {
            // Vraag om bevestiging als er wijzigingen zijn gemaakt
            MessageBoxResult result = MessageBox.Show(
                "Weet u zeker dat u wilt annuleren? Eventuele wijzigingen gaan verloren.", 
                "Annuleren", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                NavigationService.GoBack();
            }
        }

        private void BtnTerug_Click(object sender, RoutedEventArgs e)
        {
            // Zelfde functionaliteit als annuleren
            BtnAnnuleren_Click(sender, e);
        }
    }
} 