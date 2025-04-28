using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfZalaStock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Product> _alleProducten;
        private List<Product> _gefilterdeProdukten;
        private StockBeheer _stockBeheer;
        private Product _geselecteerdProduct;

        public MainWindow()
        {
            InitializeComponent();
            InitialiseerData();
            InitialiseerUI();
        }

        private void InitialiseerData()
        {
            _stockBeheer = new StockBeheer();

            string csvBestandsPad = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "producten.csv");
            _alleProducten = ProductParser.LeesProducten(csvBestandsPad);
            _gefilterdeProdukten = new List<Product>();
        }

        private void InitialiseerUI()
        {
            // Vul de categorie dropdown
            List<string> categorieën = new List<string>
            {
                "Alle",
                "Kleding",
                "Schoenen",
                "Sieraden"
            };
            CmbCategorie.ItemsSource = categorieën;
            CmbCategorie.SelectedIndex = 0;

            // Formaat voor verkochte en geretourneerde items
            LstVerkocht.ItemTemplate = MaakProductItemTemplate();
            LstGeretourneerd.ItemTemplate = MaakProductItemTemplate();

            // Update UI
            UpdateProductenLijst();
            UpdateTotalen();
        }

        private DataTemplate MaakProductItemTemplate()
        {
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetBinding(TextBlock.TextProperty, new Binding());
            return new DataTemplate { VisualTree = factory };
        }

        private void UpdateProductenLijst()
        {
            string geselecteerdeCategorie = CmbCategorie.SelectedItem as string;

            if (geselecteerdeCategorie == "Alle")
            {
                _gefilterdeProdukten = _alleProducten;
            }
            else
            {
                _gefilterdeProdukten = _alleProducten.Where(p =>
                {
                    if (geselecteerdeCategorie == "Kleding" && p is Kleding)
                        return true;
                    if (geselecteerdeCategorie == "Schoenen" && p is Schoen)
                        return true;
                    if (geselecteerdeCategorie == "Sieraden" && p is Sieraad)
                        return true;
                    return false;
                }).ToList();
            }

            LstProducten.ItemsSource = null;
            LstProducten.ItemsSource = _gefilterdeProdukten;
        }

        private void UpdateProductDetails()
        {
            if (_geselecteerdProduct != null)
            {
                string prijsFormatted = _geselecteerdProduct.Prijs.ToString("0.00").Replace(".", ",");
                string details = $"{_geselecteerdProduct.Naam} ({_geselecteerdProduct.Merk}) - €{prijsFormatted}";
                TxtNaam.Text = details;
                TxtVoorraad.Text = $"In Stock: {_geselecteerdProduct.AantalInStock}";

                // Knoppen altijd ingeschakeld laten
                BtnVerkopen.IsEnabled = true;
                BtnRetourneren.IsEnabled = true;
            }
            else
            {
                TxtNaam.Text = "Selecteer een product...";
                TxtVoorraad.Text = "In Stock: 0";
                
                // Knoppen altijd ingeschakeld laten
                BtnVerkopen.IsEnabled = true;
                BtnRetourneren.IsEnabled = true;
            }
        }

        private bool IsProductVerkocht(Product product)
        {
            return _stockBeheer.Verkocht.ContainsKey(product) && _stockBeheer.Verkocht[product] > 0;
        }

        private void UpdateVerkochteLijst()
        {
            List<string> verkochteItems = new List<string>();
            foreach (var kv in _stockBeheer.Verkocht)
            {
                string prijsFormatted = kv.Key.Prijs.ToString("0.00").Replace(".", ",");
                decimal totaal = kv.Key.Prijs * kv.Value;
                string totaalFormatted = totaal.ToString("0.00").Replace(".", ",");
                verkochteItems.Add($"{kv.Key.Naam} ({kv.Key.Merk}) - €{prijsFormatted} x {kv.Value} - Totaal: {totaalFormatted}");
            }
            LstVerkocht.ItemsSource = verkochteItems;
        }

        private void UpdateGeretourneerdeLijst()
        {
            List<string> geretourneerdeItems = new List<string>();
            foreach (var kv in _stockBeheer.Geretourneerd)
            {
                string prijsFormatted = kv.Key.Prijs.ToString("0.00").Replace(".", ",");
                decimal totaal = kv.Key.Prijs * kv.Value;
                string totaalFormatted = totaal.ToString("0.00").Replace(".", ",");
                geretourneerdeItems.Add($"{kv.Key.Naam} ({kv.Key.Merk}) - €{prijsFormatted} x {kv.Value} - Totaal: {totaalFormatted}");
            }
            LstGeretourneerd.ItemsSource = geretourneerdeItems;
        }

        private void UpdateTotalen()
        {
            decimal totaalVerkocht = _stockBeheer.BerekenTotaalVerkocht();
            decimal totaalGeretourneerd = _stockBeheer.BerekenTotaalGeretourneerd();
            decimal netto = _stockBeheer.BerekenNetto();

            string totaalVerkochtFormatted = totaalVerkocht.ToString("0.00").Replace(".", ",");
            string totaalGeretourneerdFormatted = totaalGeretourneerd.ToString("0.00").Replace(".", ",");
            string nettoFormatted = netto.ToString("0.00").Replace(".", ",");

            TxtTotaalVerkocht.Text = $"Totaalbedrag verkopen: € {totaalVerkochtFormatted}";
            TxtTotaalRetours.Text = $"Totaalbedrag retours: -€ {totaalGeretourneerdFormatted}";
            TxtTotaalBedrag.Text = $"Totaalbedrag: € {nettoFormatted}";
        }

        private int GetAantal()
        {
            if (int.TryParse(TxtAantal.Text, out int aantal) && aantal > 0)
            {
                return aantal;
            }
            MessageBox.Show("Voer een geldig aantal in (geheel getal groter dan 0).", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
            return 0;
        }

        private void CmbCategorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProductenLijst();
            _geselecteerdProduct = null;
            UpdateProductDetails();
        }

        private void LstProducten_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _geselecteerdProduct = LstProducten.SelectedItem as Product;
            UpdateProductDetails();
        }

        private void BtnVerkopen_Click(object sender, RoutedEventArgs e)
        {
            if (_geselecteerdProduct == null)
            {
                MessageBox.Show("Selecteer eerst een product.", "Informatie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_geselecteerdProduct.AantalInStock <= 0)
            {
                MessageBox.Show("Dit product is niet meer op voorraad.", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int aantal = GetAantal();
            if (aantal <= 0) return;

            if (_geselecteerdProduct.AantalInStock < aantal)
            {
                MessageBox.Show($"Er zijn slechts {_geselecteerdProduct.AantalInStock} exemplaren beschikbaar.", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_stockBeheer.Verkopen(_geselecteerdProduct, aantal))
            {
                UpdateProductDetails();
                UpdateVerkochteLijst();
                UpdateTotalen();
            }
            else
            {
                MessageBox.Show("Verkoop kon niet worden verwerkt.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRetourneren_Click(object sender, RoutedEventArgs e)
        {
            if (_geselecteerdProduct == null)
            {
                MessageBox.Show("Selecteer eerst een product.", "Informatie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int aantal = GetAantal();
            if (aantal <= 0) return;

            if (!_stockBeheer.Verkocht.ContainsKey(_geselecteerdProduct) || _stockBeheer.Verkocht[_geselecteerdProduct] < aantal)
            {
                MessageBox.Show($"Er zijn niet genoeg exemplaren verkocht om te retourneren.", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_stockBeheer.Retourneren(_geselecteerdProduct, aantal))
            {
                UpdateProductDetails();
                UpdateGeretourneerdeLijst();
                UpdateTotalen();
            }
            else
            {
                MessageBox.Show("Retour kon niet worden verwerkt.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}