using System;
using System.Windows;
using System.Windows.Controls;

namespace BenchmarkTool.CompanyApp
{
    /// <summary>
    /// Hoofdvenster voor de bedrijfsapplicatie
    /// </summary>
    public partial class CompanyMainWindow : Window
    {
        public CompanyMainWindow()
        {
            InitializeComponent();
            // Navigeer naar de loginpagina bij het opstarten
            MainFrame.Navigate(new Uri("/Pages/CompanyLoginPage.xaml", UriKind.Relative));
        }

        // Methode om de bedrijfsnaam en logo in te stellen na inloggen
        public void SetBedrijfsgegevens(string naam, byte[] logoData = null)
        {
            txtBedrijfsnaam.Text = naam;

            if (logoData != null && logoData.Length > 0)
            {
                try
                {
                    var image = new System.Windows.Media.Imaging.BitmapImage();
                    using (var ms = new System.IO.MemoryStream(logoData))
                    {
                        image.BeginInit();
                        image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                    }
                    imgLogo.Source = image;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij laden van logo: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Navigeert naar een specifieke pagina
        /// </summary>
        /// <param name="pagina">Het type Page waarnaar genavigeerd moet worden</param>
        public void NavigeerNaar(Page pagina)
        {
            MainFrame.Navigate(pagina);
        }

        /// <summary>
        /// Navigeert naar een pagina via URI pad
        /// </summary>
        /// <param name="paginaPad">URI pad naar de pagina</param>
        public void NavigeerNaar(string paginaPad)
        {
            MainFrame.Navigate(new Uri(paginaPad, UriKind.Relative));
        }
    }
}