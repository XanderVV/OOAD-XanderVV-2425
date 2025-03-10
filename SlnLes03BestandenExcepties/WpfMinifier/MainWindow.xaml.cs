using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace WpfMinifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void Txtbox_LostFocus(object sender, RoutedEventArgs e)
        {
            string directoryPath = this.txtbox.Text;
            this.Controle(directoryPath);
        }

        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void Selecteerbtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                SelectedPath = this.geselecteerdeMapPad,
                Description = "Selecteer een map met CSS/HTML/JS bestanden",
            };

            if (dialog.ShowDialog() == true)
            {
                this.Controle(dialog.SelectedPath);
            }
            else
            {
                this.lblMessage.Content = "Mapselectie is geannuleerd.";
                this.lblMessage.Foreground = Brushes.Red;
            }
        }

        /// <param name="directoryPath">The path to validate.</param>
        private void Controle(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                this.geselecteerdeMapPad = directoryPath;
                this.txtbox.Text = this.geselecteerdeMapPad;
                this.VernieuwBestandenlijst();
                this.lblMessage.Content = string.Empty;
            }
            else
            {
                this.lblMessage.Content = "De opgegeven map bestaat niet.";
                this.lblMessage.Foreground = Brushes.Red;
            }
        }

        private void VernieuwBestandenlijst()
        {
            this.Bestandenlst.Items.Clear();
            string[] files;
            try
            {
                files = Directory.GetFiles(this.geselecteerdeMapPad, "*.*", SearchOption.TopDirectoryOnly);
            }
            catch (IOException ex)
            {
                this.lblMessage.Content = $"Fout bij het toegang krijgen tot bestanden in {this.geselecteerdeMapPad}: {ex.Message}";
                this.lblMessage.Foreground = Brushes.Red;
                return;
            }
            catch (Exception ex)
            {
                this.lblMessage.Content = $"Onbekende fout bij het lezen van {this.geselecteerdeMapPad}: {ex.Message}";
                this.lblMessage.Foreground = Brushes.Red;
                return;
            }

            string[] toegestaneExtensies = { ".css", ".html", ".js" };
            foreach (string filePath in files)
            {
                string bestandsExtensie = Path.GetExtension(filePath);
                if (toegestaneExtensies.Contains(bestandsExtensie))
                {
                    this.Bestandenlst.Items.Add(filePath);
                }
            }
        }

        private void Knopen()
        {
            bool isItemGeselecteerd = this.Bestandenlst.SelectedItem != null;
            bool isAlGeMinified = false;

            if (isItemGeselecteerd)
            {
                string selectedItem = (string)this.Bestandenlst.SelectedItem;
                isAlGeMinified = selectedItem.Contains(".min.");
            }

            this.Minifyalsbtn.IsEnabled = isItemGeselecteerd && !isAlGeMinified;
            this.Minifybtn.IsEnabled = isItemGeselecteerd && !isAlGeMinified;
        }

        private string MinifyHtml(string html) =>
            Regex.Replace(
                Regex.Replace(
                    html,
                    @">\s+<",
                    "><"),
                @"\s+",
                " ");

        private string MinifyCss(string css) =>
            Regex.Replace(
                Regex.Replace(
                    Regex.Replace(
                        css,
                        @"\/\*(.*?)\*\/",
                        string.Empty),
                    @"\s+",
                    " "),
                @"\s*([:,;{}])\s*",
                "$1");

        private string MinifyJavaScript(string js) =>
            Regex.Replace(
                Regex.Replace(
                    Regex.Replace(
                        js,
                        @"\/\/.*$",
                        string.Empty,
                        RegexOptions.Multiline),
                    @"\/\*.*?\*\/",
                    string.Empty,
                    RegexOptions.Singleline),
                @"\s+",
                " ")
            .Replace(@"\s*([:,;{}()=|&])\s*", "$1");

        private string Minify(string filePath)
        {
            string fileContent;
            try
            {
                fileContent = File.ReadAllText(filePath);
            }
            catch (IOException)
            {
                this.lblMessage.Content = "Fout bij het laden";
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
            this.geselecteerdItem = (string)this.Bestandenlst.SelectedItem;
            int laastePunt = this.geselecteerdItem.LastIndexOf('.');
            string fileNaamZonderExtension = this.geselecteerdItem.Substring(0, laastePunt);
            string fileExtension = this.geselecteerdItem.Substring(laastePunt);
            string eindResultaat = $"{fileNaamZonderExtension}.min{fileExtension}";
            return eindResultaat;
        }

        private void Minifybtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.Bestandenlst.SelectedItem != null)
            {
                ListBoxItem newItem = new ListBoxItem { Content = this.Minfy() };
                this.Bestandenlst.Items.Add(newItem.Content);
                this.Knopen();
            }
        }

        private void Bestandenlst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Knopen();
        }

        private void Minifyalsbtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.Bestandenlst.SelectedItem != null)
            {
                string origineleFilePath = (string)this.Bestandenlst.SelectedItem;
                string minifiedContent = this.Minify(origineleFilePath);
                if (minifiedContent == null)
                {
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    FileName = this.Minfy(),
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, minifiedContent);
                    }
                    catch (IOException)
                    {
                        this.lblMessage.Content = "fout bij het laden";
                    }

                    this.Knopen();
                }
            }
        }
    }
}
