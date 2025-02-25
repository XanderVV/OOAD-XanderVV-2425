using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfMinifier
{
    public partial class MainWindow : Window
    {
        // Pad naar de momenteel geselecteerde map
        private string geselecteerdeMapPad;
        // Huidig geselecteerd item in de bestandslijst
        private string geselecteerdItem;

        public MainWindow()
        {
            InitializeComponent();
            geselecteerdeMapPad = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            VernieuwBestandenlijst();
        }

        // ----------------------------------
        // ADD THIS METHOD FOR LOSTFOCUS
        // ----------------------------------
        private void Txtbox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Example: re-validate directory when user leaves the textbox
            string directoryPath = txtbox.Text;
            Controle(directoryPath);
        }

        private void Selecteerbtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                SelectedPath = geselecteerdeMapPad,
                Description = "Selecteer een map met CSS/HTML/JS bestanden"
            };

            if (dialog.ShowDialog() == true)
            {
                Controle(dialog.SelectedPath);
            }
            else
            {
                lblMessage.Content = "Mapselectie is geannuleerd.";
                lblMessage.Foreground = Brushes.Red;
            }
        }

        private void Controle(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                geselecteerdeMapPad = directoryPath;
                txtbox.Text = geselecteerdeMapPad;
                VernieuwBestandenlijst();
                lblMessage.Content = "";
            }
            else
            {
                lblMessage.Content = "De opgegeven map bestaat niet.";
                lblMessage.Foreground = Brushes.Red;
            }
        }

        private void VernieuwBestandenlijst()
        {
            Bestandenlst.Items.Clear();
            string[] files = null;
            try
            {
                files = Directory.GetFiles(geselecteerdeMapPad, "*.*", SearchOption.TopDirectoryOnly);
            }
            catch (IOException e)
            {
                lblMessage.Content = $"Fout bij het toegang krijgen tot bestanden in {geselecteerdeMapPad}: {e.Message}";
                lblMessage.Foreground = Brushes.Red;
                return;
            }
            catch (Exception e)
            {
                lblMessage.Content = $"Onbekende fout bij het lezen van {geselecteerdeMapPad}: {e.Message}";
                lblMessage.Foreground = Brushes.Red;
                return;
            }

            string[] toegestaneExtensies = { ".css", ".html", ".js" };
            foreach (string filePath in files)
            {
                string bestandsExtensie = Path.GetExtension(filePath);
                if (toegestaneExtensies.Contains(bestandsExtensie))
                {
                    Bestandenlst.Items.Add(filePath);
                }
            }
        }

        private void Knopen()
        {
            bool isItemGeselecteerd = Bestandenlst.SelectedItem != null;
            bool isAlGeMinified = false;

            if (isItemGeselecteerd)
            {
                string selectedItem = (string)Bestandenlst.SelectedItem;
                isAlGeMinified = selectedItem.Contains(".min.");
            }
            Minifyalsbtn.IsEnabled = isItemGeselecteerd && !isAlGeMinified;
            Minifybtn.IsEnabled = isItemGeselecteerd && !isAlGeMinified;
        }

        // Minimizer for HTML, CSS, and JS
        private string MinifyHtml(string html) =>
            Regex.Replace(Regex.Replace(html, @">\s+<", "><"), @"\s+", " ");

        private string MinifyCss(string css) =>
            Regex.Replace(Regex.Replace(
                Regex.Replace(css, @"\/\*(.*?)\*\/", ""), @"\s+", " "),
                @"\s*([:,;{}])\s*", "$1");

        private string MinifyJavaScript(string js) =>
            Regex.Replace(
                Regex.Replace(
                    Regex.Replace(js, @"\/\/.*$", "", RegexOptions.Multiline),
                    @"\/\*.*?\*\/", "", RegexOptions.Singleline),
                @"\s+", " "
            ).Replace(@"\s*([:,;{}()=|&])\s*", "$1");

        private string Minify(string filePath)
        {
            string fileContent;
            try
            {
                fileContent = File.ReadAllText(filePath);
            }
            catch (IOException)
            {
                lblMessage.Content = "fout bij het laden";
                return null;
            }

            string fileExtension = Path.GetExtension(filePath).ToLower();
            switch (fileExtension)
            {
                case ".html":
                    return MinifyHtml(fileContent);
                case ".css":
                    return MinifyCss(fileContent);
                case ".js":
                    return MinifyJavaScript(fileContent);
                default:
                    return fileContent;
            }
        }

        private string Minfy()
        {
            geselecteerdItem = (string)Bestandenlst.SelectedItem;
            int laastePunt = geselecteerdItem.LastIndexOf('.');
            string fileNaamZonderExtension = geselecteerdItem.Substring(0, laastePunt);
            string fileExtension = geselecteerdItem.Substring(laastePunt);
            string eindResultaat = $"{fileNaamZonderExtension}.min{fileExtension}";
            return eindResultaat;
        }

        private void Minifybtn_Click(object sender, RoutedEventArgs e)
        {
            if (Bestandenlst.SelectedItem != null)
            {
                ListBoxItem newItem = new ListBoxItem { Content = Minfy() };
                Bestandenlst.Items.Add(newItem.Content);
                Knopen();
            }
        }

        private void Bestandenlst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Knopen();
        }

        private void Minifyalsbtn_Click(object sender, RoutedEventArgs e)
        {
            if (Bestandenlst.SelectedItem != null)
            {
                string origineleFilePath = (string)Bestandenlst.SelectedItem;
                string minifiedContent = Minify(origineleFilePath);
                if (minifiedContent == null) return;

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = Minfy()
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, minifiedContent);
                    }
                    catch (IOException)
                    {
                        lblMessage.Content = "fout bij het laden";
                    }
                    Knopen();
                }
            }
        }
    }
}
