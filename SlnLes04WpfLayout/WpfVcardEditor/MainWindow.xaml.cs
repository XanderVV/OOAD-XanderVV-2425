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
        private string? huidigeFilePath = null; // Het pad naar het huidige geopende bestand, indien aanwezig
        private bool isCardVerandert = false; // bijhouden of de vCard is gewijzigd
        public MainWindow()
        {
            InitializeComponent();
            txtFirstname.TextChanged += Card_Verandert;
            txtLastname.TextChanged += Card_Verandert;
            datBirthday.SelectedDateChanged += Card_DateVeranderen;
            manRb.Checked += Card_Verandert;
            vrouwRb.Checked += Card_Verandert;
            onbekendRb.Checked += Card_Verandert;
            txtEmail.TextChanged += Card_Verandert;
            txtPhone.TextChanged += Card_Verandert;
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
            OpenFileDialog dialog = new ()
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
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("N;CHARSET=UTF-8:"))
                    {
                        string nameSection = line["N;CHARSET=UTF-8:".Length..];
                        string[] nameParts = nameSection.Split(';');

                        if (nameParts.Length > 1)
                        {
                            txtLastname.Text = nameParts[0]; 
                            txtFirstname.Text = nameParts[1]; 
                        }
                    }
                    else if (line.StartsWith("BDAY:"))
                    {
                        string birthdayStr = line["BDAY:".Length..];

                        // link https://stackoverflow.com/questions/43081866/set-specific-date-with-string-to-datetimepicker#:~:text=You%20can%20use%20DateTime.,Value%20%3D%20DateTime. 
                        if (DateTime.TryParseExact(birthdayStr, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime birthday))
                        {
                            datBirthday.SelectedDate = birthday;
                        }
                    }
                    else if (line.StartsWith("GENDER:"))
                    {
                        string gender = line["GENDER:".Length..];
                        manRb.IsChecked = gender == "M";
                        vrouwRb.IsChecked = gender == "F";
                    }
                    else if (line.StartsWith("EMAIL;CHARSET=UTF-8;type=WORK,INTERNET:"))
                    {
                        txtEmail.Text = line["EMAIL;CHARSET=UTF-8;type=WORK,INTERNET:".Length..];
                    }
                    else if (line.StartsWith("TEL;TYPE=HOME,VOICE:"))
                    {
                        txtPhone.Text = line["TEL;TYPE=HOME,VOICE:".Length..];
                    }
                    else if (line.StartsWith("PHOTO;ENCODING=BASE64;TYPE=image/jpeg:"))
                    {
                        string base64Image = line["PHOTO;ENCODING=BASE64;TYPE=image/jpeg:".Length..];
                        fotoimg.Source = ConvertBase64ToBitmapImage(base64Image.Trim());
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
            string vCard = "BEGIN:VCARD\n" +
                "VERSION:3.0\n";

            // Voornaam && achternaam
            if (!string.IsNullOrEmpty(txtLastname.Text) || !string.IsNullOrEmpty(txtFirstname.Text))
            {
                string lastname = txtLastname.Text ?? "";
                string firstname = txtFirstname.Text ?? "";

                vCard += $"N;CHARSET=UTF-8:{lastname};{firstname}\n";
            }

            // Geboortedatum
            if (datBirthday.SelectedDate.HasValue)
            {
                string birthday = datBirthday.SelectedDate.Value.ToString("yyyyMMdd");
                vCard += $"BDAY:{birthday}\n";
            }

            // Geslacht
            if (manRb.IsChecked == true)
            {
                vCard += "GENDER:M\n";
            }
            else if (vrouwRb.IsChecked == true)
            {
                vCard += "GENDER:F\n";
            }

            // Email
            if (!string.IsNullOrEmpty(txtEmail.Text))
            {
                vCard += $"EMAIL;CHARSET=UTF-8;type=WORK,INTERNET:{txtEmail.Text}\n";
            }

            // Telefoonnummer
            if (!string.IsNullOrEmpty(txtPhone.Text))
            {
                vCard += $"TEL;TYPE=HOME,VOICE:{txtPhone.Text}\n";
            }

            vCard += $"PHOTO;ENCODING=BASE64;TYPE=image/jpeg:{ImageToBase64(fotoimg.Source as BitmapImage)}\n";
            vCard += "END:VCARD\n";

            File.WriteAllText(filepath, vCard.ToString());
        }

        private void Btn_Save(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(huidigeFilePath))
            {
                SaveVCard(huidigeFilePath);
                MessageBox.Show("Bestand succesvol opgeslagen.", "Bevestiging", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new ()
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

        // Markeer dat de vCard is veranderd als de datum is gewijzigd .
        private void Card_DateVeranderen(object? sender, SelectionChangedEventArgs e)
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
        }

        private void ClearAllVelden()
        {
            // Clear de velden hier
            txtFirstname.Clear();
            txtLastname.Clear();
            datBirthday.SelectedDate = null;
            manRb.IsChecked = false;
            vrouwRb.IsChecked = false;
            txtEmail.Clear();
            txtPhone.Clear();
            fotoimg.Source = null;
        }

        // met hulp van chatgpt
        private string ImageToBase64(BitmapImage imageSource)
        {
            ArgumentNullException.ThrowIfNull(imageSource);

            JpegBitmapEncoder encoder = new ();
            encoder.Frames.Add(BitmapFrame.Create(imageSource));

            using MemoryStream ms = new ();
            encoder.Save(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        private void Fotobtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new ()
            {
                Filter = "JPEG Files (*.jpeg;*.jpg)|*.jpeg;*.jpg",
                Title = "Selecteer een afbeelding"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string bestandsnaam = openFileDialog.FileName;
                BitmapImage bitmap = new (new Uri(bestandsnaam));
                fotoimg.Source = bitmap;
            }
        }

        private void UpdateSaveButtonStatus()
        {
            BtnSave.IsEnabled = isCardVerandert && huidigeFilePath != null;
        }

        // Update de statusbalk met informatie over het huidig geopende bestand.
        private void UpdateCompletionStatus()
        {
            int totalVakken = 7;
            int gevuldVakken = 0;

            if (!string.IsNullOrEmpty(txtFirstname.Text)) gevuldVakken++;
            if (!string.IsNullOrEmpty(txtLastname.Text)) gevuldVakken++;
            if (datBirthday.SelectedDate.HasValue) gevuldVakken++;
            if (manRb.IsChecked == true || vrouwRb.IsChecked == true) gevuldVakken++;
            if (!string.IsNullOrEmpty(txtEmail.Text)) gevuldVakken++;
            if (!string.IsNullOrEmpty(txtPhone.Text)) gevuldVakken++;
            if (fotoimg.Source != null) gevuldVakken++;

            double percentage = (double)gevuldVakken / totalVakken * 100;
            statusbarPercentage.Content = $"Percentage ingevuld: {percentage:0}%";
        }

        // met hulp van chatgpt
        private BitmapImage ConvertBase64ToBitmapImage(string base64String)
        {
            BitmapImage bitmapImage = new ();
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using MemoryStream ms = new (imageBytes);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error converting Base64 to BitmapImage: {ex.Message}");
            }
            return bitmapImage;
        }
    }
}