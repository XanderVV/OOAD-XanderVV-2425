using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace WpfVcardEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string huidigeFilePath = null; // Het pad naar het huidige geopende bestand, indien aanwezig
        private bool isCardVerandert = false; // bijhouden of de vCard is gewijzigd
        private VCard currentCard; // Object to hold the current VCard data

        public MainWindow()
        {
            InitializeComponent();
            currentCard = new VCard(); // Initialize with an empty VCard
            txtFirstname.TextChanged += Card_Verandert;
            txtLastname.TextChanged += Card_Verandert;
            datBirthday.SelectedDateChanged += Card_DateVeranderen;
            manRb.Checked += Card_Verandert;
            vrouwRb.Checked += Card_Verandert;
            onbekendRb.Checked += Card_Verandert;
            txtEmail.TextChanged += Card_Verandert;
            txtPhone.TextChanged += Card_Verandert;
            txtBedrijf.TextChanged += Card_Verandert;
            txtjob.TextChanged += Card_Verandert;
            txtWorkMail.TextChanged += Card_Verandert;
            txtworktele.TextChanged += Card_Verandert;
            txtLinkedIn.TextChanged += Card_Verandert;
            txtFacebook.TextChanged += Card_Verandert;
            txtInstagram.TextChanged += Card_Verandert;
            txtYoutube.TextChanged += Card_Verandert;
        }
        private void Btn_About(object sender, RoutedEventArgs e)
        {
            new AboutWindow().Show();
        }

        private void Btn_exit(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Ben je zeker dat je de applicatie wil afsluiten?", "Toepassing sluiten", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.OK)
            {
                Environment.Exit(0);
            }
        }

        // Handelt het 'Open' commando af: laat een dialoogvenster zien om een vCard te kiezen.
        private void Btn_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "vCard files (*.vcf)|*.vcf"
            };

            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult == true)
            {
                ProcessVCardFile(dialog.FileName);
            }
            else
            {
                MessageBox.Show("Het bestand is helaas niet gevonden", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Verwerkt het gekozen vCard bestand, leest de inhoud en vult alles in.
        private void ProcessVCardFile(string filePath)
        {
            try
            {
                currentCard = new VCard(); // Reset with a new VCard
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("N;CHARSET=UTF-8:"))
                    {
                        string nameSection = line.Substring("N;CHARSET=UTF-8:".Length);
                        string[] nameParts = nameSection.Split(';');

                        if (nameParts.Length > 1)
                        {
                            currentCard.LastName = nameParts[0];
                            currentCard.FirstName = nameParts[1];
                            txtLastname.Text = currentCard.LastName;
                            txtFirstname.Text = currentCard.FirstName;
                        }
                    }
                    else if (line.StartsWith("BDAY:"))
                    {
                        string birthdayStr = line.Substring("BDAY:".Length);

                        if (DateTime.TryParseExact(birthdayStr, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime birthday))
                        {
                            currentCard.BirthDay = birthday;
                            datBirthday.SelectedDate = birthday;
                        }
                    }
                    else if (line.StartsWith("GENDER:"))
                    {
                        string gender = line.Substring("GENDER:".Length);
                        currentCard.Gender = gender;
                        manRb.IsChecked = gender == "M";
                        vrouwRb.IsChecked = gender == "F";
                        onbekendRb.IsChecked = gender != "M" && gender != "F";
                    }
                    else if (line.StartsWith("EMAIL;CHARSET=UTF-8;type=WORK,INTERNET:"))
                    {
                        currentCard.Email = line.Substring("EMAIL;CHARSET=UTF-8;type=WORK,INTERNET:".Length);
                        txtEmail.Text = currentCard.Email;
                    }
                    else if (line.StartsWith("TEL;TYPE=HOME,VOICE:"))
                    {
                        currentCard.Phone = line.Substring("TEL;TYPE=HOME,VOICE:".Length);
                        txtPhone.Text = currentCard.Phone;
                    }
                    else if (line.StartsWith("PHOTO;ENCODING=BASE64;TYPE=image/jpeg:"))
                    {
                        string base64Image = line.Substring("PHOTO;ENCODING=BASE64;TYPE=image/jpeg:".Length);
                        BitmapImage bitmapImage = ConvertBase64ToBitmapImage(base64Image.Trim());
                        currentCard.Photo = bitmapImage;
                        fotoimg.Source = bitmapImage;
                    }
                    else if (line.StartsWith("ORG:"))
                    {
                        currentCard.Company = line.Substring("ORG:".Length);
                        txtBedrijf.Text = currentCard.Company;
                    }
                    else if (line.StartsWith("TITLE:"))
                    {
                        currentCard.JobTitle = line.Substring("TITLE:".Length);
                        txtjob.Text = currentCard.JobTitle;
                    }
                    else if (line.StartsWith("EMAIL;TYPE=WORK:"))
                    {
                        currentCard.WorkEmail = line.Substring("EMAIL;TYPE=WORK:".Length);
                        txtWorkMail.Text = currentCard.WorkEmail;
                    }
                    else if (line.StartsWith("TEL;TYPE=WORK,VOICE:"))
                    {
                        currentCard.WorkPhone = line.Substring("TEL;TYPE=WORK,VOICE:".Length);
                        txtworktele.Text = currentCard.WorkPhone;
                    }
                    else if (line.StartsWith("URL;TYPE=LinkedIn:"))
                    {
                        currentCard.LinkedIn = line.Substring("URL;TYPE=LinkedIn:".Length);
                        txtLinkedIn.Text = currentCard.LinkedIn;
                    }
                    else if (line.StartsWith("URL;TYPE=Facebook:"))
                    {
                        currentCard.Facebook = line.Substring("URL;TYPE=Facebook:".Length);
                        txtFacebook.Text = currentCard.Facebook;
                    }
                    else if (line.StartsWith("URL;TYPE=Instagram:"))
                    {
                        currentCard.Instagram = line.Substring("URL;TYPE=Instagram:".Length);
                        txtInstagram.Text = currentCard.Instagram;
                    }
                    else if (line.StartsWith("URL;TYPE=YouTube:"))
                    {
                        currentCard.YouTube = line.Substring("URL;TYPE=YouTube:".Length);
                        txtYoutube.Text = currentCard.YouTube;
                    }
                    statusbar.Content = $"Huidige kaart: {Path.GetFileName(filePath)}";
                }
                isCardVerandert = false;
                huidigeFilePath = filePath;
                UpdateCompletionStatus(); // Roept de methode aan die het percentage ingevuld berekent
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kan het bestand niet lezen {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Sla de info van de vCard op in een bestand.
        private void SaveVCard(string filepath)
        {
            // Get the form values and update the VCard object
            UpdateCardFromForm();
            
            // Generate the VCF code using the method from our VCard class
            string vcfCode = currentCard.GenerateVcfCode();
            
            // Save to file
            File.WriteAllText(filepath, vcfCode);
        }

        // Update the VCard object with values from the form
        private void UpdateCardFromForm()
        {
            // Personal information
            currentCard.FirstName = txtFirstname.Text;
            currentCard.LastName = txtLastname.Text;
            currentCard.BirthDay = datBirthday.SelectedDate;
            
            // Gender
            if (manRb.IsChecked == true)
                currentCard.Gender = "M";
            else if (vrouwRb.IsChecked == true)
                currentCard.Gender = "F";
            else
                currentCard.Gender = ""; // or a default value
            
            currentCard.Email = txtEmail.Text;
            currentCard.Phone = txtPhone.Text;
            currentCard.Photo = fotoimg.Source as BitmapImage;
            
            // Work information
            currentCard.Company = txtBedrijf.Text;
            currentCard.JobTitle = txtjob.Text;
            currentCard.WorkEmail = txtWorkMail.Text;
            currentCard.WorkPhone = txtworktele.Text;
            
            // Social media
            currentCard.LinkedIn = txtLinkedIn.Text;
            currentCard.Facebook = txtFacebook.Text;
            currentCard.Instagram = txtInstagram.Text;
            currentCard.YouTube = txtYoutube.Text;
        }

        private void Btn_Save(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(huidigeFilePath))
            {
                SaveVCard(huidigeFilePath);
                MessageBox.Show("Bestand succesvol opgeslagen.", "Bevestiging", MessageBoxButton.OK, MessageBoxImage.Information);
                isCardVerandert = false;
                UpdateSaveButtonStatus();
            }
        }

        private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "vCard files (*.vcf)|*.vcf|All files (*.*)|*.*",
                Title = "Save vCard As"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    SaveVCard(saveFileDialog.FileName);
                    huidigeFilePath = saveFileDialog.FileName;
                    isCardVerandert = false;
                    UpdateSaveButtonStatus();
                    statusbar.Content = $"Huidige kaart: {Path.GetFileName(huidigeFilePath)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Er is een fout opgetreden bij het opslaan van het bestand als: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
  
        // Als iemand iets aanpast, onthoud dat en kijk of de opslaanknop aan moet.
        private void Card_Verandert(object sender, EventArgs e)
        {
            isCardVerandert = true;
            UpdateSaveButtonStatus();
        }

        // Markeer dat de vCard is veranderd als de datum is gewijzigd.
        private void Card_DateVeranderen(object sender, SelectionChangedEventArgs e)
        {
            isCardVerandert = true;
            UpdateSaveButtonStatus();
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            if (isCardVerandert)
            {
                MessageBoxResult result = MessageBox.Show("Je hebt onopgeslagen wijzigingen. Wil je een nieuwe kaart maken en de wijzigingen verliezen?", "Nieuwe vCard", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            ClearAllVelden();
            huidigeFilePath = null;
            isCardVerandert = false;
            statusbar.Content = "huidige kaart: (geen geopend)";
            UpdateSaveButtonStatus();
        }

        private void ClearAllVelden()
        {
            // Create a new empty VCard
            currentCard = new VCard();
            
            // Clear the form fields
            txtFirstname.Clear();
            txtLastname.Clear();
            datBirthday.SelectedDate = null;
            manRb.IsChecked = false;
            vrouwRb.IsChecked = false;
            onbekendRb.IsChecked = false;
            txtEmail.Clear();
            txtPhone.Clear();
            fotoimg.Source = null;
            txtBedrijf.Clear();
            txtjob.Clear();
            txtWorkMail.Clear();
            txtworktele.Clear();
            txtLinkedIn.Clear();
            txtFacebook.Clear();
            txtInstagram.Clear();
            txtYoutube.Clear();
        }

        // met hulp van chatgpt
        private string ImageToBase64(BitmapImage imageSource)
        {
            if (imageSource == null)
                return string.Empty;

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageSource));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private void Fotobtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif, *.bmp) | *.jpg; *.jpeg; *.png; *.gif; *.bmp",
                Title = "Open een afbeelding"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                fotoimg.Source = bitmapImage;
                currentCard.Photo = bitmapImage;
                isCardVerandert = true;
                UpdateSaveButtonStatus();
            }
        }

        private void UpdateSaveButtonStatus()
        {
            BtnSave.IsEnabled = isCardVerandert && !string.IsNullOrEmpty(huidigeFilePath);
            UpdateCompletionStatus();
        }

        private void UpdateCompletionStatus()
        {
            // Update card from form before calculating completion
            UpdateCardFromForm();
            
            // Count filled fields
            int filledFields = 0;
            int totalFields = 13; // Update this number based on the actual fields you want to count

            if (!string.IsNullOrEmpty(currentCard.FirstName)) filledFields++;
            if (!string.IsNullOrEmpty(currentCard.LastName)) filledFields++;
            if (currentCard.BirthDay.HasValue) filledFields++;
            if (!string.IsNullOrEmpty(currentCard.Gender)) filledFields++;
            if (!string.IsNullOrEmpty(currentCard.Email)) filledFields++;
            if (!string.IsNullOrEmpty(currentCard.Phone)) filledFields++;
            if (currentCard.Photo != null) filledFields++;
            if (!string.IsNullOrEmpty(currentCard.Company)) filledFields++;
            if (!string.IsNullOrEmpty(currentCard.JobTitle)) filledFields++;
            if (!string.IsNullOrEmpty(currentCard.WorkEmail)) filledFields++;
            if (!string.IsNullOrEmpty(currentCard.WorkPhone)) filledFields++;
            if (!string.IsNullOrEmpty(currentCard.LinkedIn)) filledFields++;
            if (!string.IsNullOrEmpty(currentCard.Facebook)) filledFields++;
            
            double percentage = (double)filledFields / totalFields * 100;
            statusbarPercentage.Content = $"percentage ingevuld: {percentage:0}%";
        }

        private BitmapImage ConvertBase64ToBitmapImage(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(imageBytes);
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}