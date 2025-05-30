using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BenchmarkTool.ClassLibrary.Models;
using BenchmarkTool.ClassLibrary.Services;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Reflection;
using System.Windows.Threading;
using System.Text;

namespace BenchmarkTool.CompanyApp.Pages
{
    /// <summary>
    /// Benchmark pagina voor vergelijking met de markt
    /// </summary>
    public partial class BenchmarkPage : Page
    {
        // Referentie naar het hoofdvenster
        private CompanyMainWindow _hoofdVenster;

        // Services
        private readonly JaarrapportService _jaarrapportService;
        private readonly BenchmarkService _benchmarkService;
        private readonly BedrijfService _bedrijfService;

        // Hulplijsten voor de UI
        private List<Categorie> _categorieën;
        private List<KostType> _kostTypes;
        private List<Vraag> _vragen;
        private List<Nacecode> _naceCodes;
        private Bedrijf _huidigBedrijf;

        // Instance variabele om benchmark data bij te houden (nodig voor latere referentie)
        private BenchmarkResultaat _huidigBenchmarkResultaat;

        // Verwijder alle tooltip-gerelateerde velden

        public BenchmarkPage()
        {
            InitializeComponent();
            
            // Verwijder tooltip initialisatie
            
            // Services initialiseren
            _jaarrapportService = new JaarrapportService();
            _benchmarkService = new BenchmarkService();
            _bedrijfService = new BedrijfService();

            // Haal het hoofdvenster op
            _hoofdVenster = (CompanyMainWindow)Application.Current.MainWindow;
            
            // Laad het huidige bedrijf
            _huidigBedrijf = App.IngelogdBedrijf;
            
            // Laad initiële data
            LaadFilterOpties();
        }

        private void LaadFilterOpties()
        {
            try
            {
                // Laad jaren
                LaadBeschikbareJaren();
                
                // Laad NACE-codes
                LaadNaceCodes();
                
                // Laad indicatoren (kosten en vragen)
                LaadIndicatoren();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden filter opties: {ex.Message}", 
                               "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaadBeschikbareJaren()
        {
            try
            {
                cmbJaar.Items.Clear();
                
                // Geen directe GetBeschikbareJaren in service, dus handmatig jaren genereren
                int huidigJaar = DateTime.Now.Year;
                List<int> jaren = new List<int>();
                
                // Voeg het huidige jaar en de afgelopen 5 jaren toe
                for (int i = 0; i <= 5; i++)
                {
                    jaren.Add(huidigJaar - i);
                }
                
                // Voeg items toe aan combo box
                foreach (var jaar in jaren)
                {
                    cmbJaar.Items.Add(jaar);
                }
                
                // Selecteer het huidige jaar als standaard
                if (cmbJaar.Items.Count > 0)
                {
                    cmbJaar.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij laden jaren: {ex.Message}");
                
                // Bij fout, handmatig jaren toevoegen
                cmbJaar.Items.Clear();
                cmbJaar.Items.Add(DateTime.Now.Year);
                cmbJaar.Items.Add(DateTime.Now.Year - 1);
                cmbJaar.Items.Add(DateTime.Now.Year - 2);
                cmbJaar.SelectedIndex = 0;
            }
        }

        private void LaadNaceCodes()
        {
            try
            {
                cmbNaceCode.Items.Clear();
                
                // Haal NACE-codes op
                _naceCodes = _bedrijfService.GetAllNacecodes();
                
                // Voeg lege optie toe voor "Alle"
                cmbNaceCode.Items.Add(new ComboBoxItem { Content = "Alle sectoren", Tag = null });
                
                // Voeg alleen niveau-2 NACE codes toe (voor overzichtelijkheid)
                var niveau2Codes = _naceCodes
                    .Where(n => n.Code.Length == 2)
                    .OrderBy(n => n.Code)
                    .ToList();
                
                foreach (var nace in niveau2Codes)
                {
                    cmbNaceCode.Items.Add(new ComboBoxItem 
                    { 
                        Content = $"{nace.Code} - {nace.Text}", 
                        Tag = nace.Code 
                    });
                }
                
                // Selecteer "Alle" als standaard
                cmbNaceCode.SelectedIndex = 0;
                
                // Als het ingelogde bedrijf een NACE-code heeft, selecteer deze
                if (_huidigBedrijf != null && !string.IsNullOrEmpty(_huidigBedrijf.NacecodeCode))
                {
                    // Neem de eerste 2 cijfers van de NACE-code voor niveau 2
                    if (_huidigBedrijf.NacecodeCode.Length >= 2)
                    {
                        string bedrijfNaceCode = _huidigBedrijf.NacecodeCode.Substring(0, 2);
                        
                        for (int i = 1; i < cmbNaceCode.Items.Count; i++)
                        {
                            var item = cmbNaceCode.Items[i] as ComboBoxItem;
                            if (item != null && item.Tag.ToString() == bedrijfNaceCode)
                            {
                                cmbNaceCode.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij laden NACE-codes: {ex.Message}");
                cmbNaceCode.Items.Add(new ComboBoxItem { Content = "Alle sectoren", Tag = null });
                cmbNaceCode.SelectedIndex = 0;
            }
        }

        private void LaadIndicatoren()
        {
            try
            {
                lstKostenIndicatoren.Items.Clear();
                lstVragenIndicatoren.Items.Clear();
                
                // Laad categorieën, kosttypen en vragen via de BedrijfService
                _categorieën = _bedrijfService.GetAllCategories();
                _kostTypes = _bedrijfService.GetAllCosttypes();
                _vragen = _bedrijfService.GetAllQuestions();
                
                // Filter vragen die voor benchmarking gebruikt kunnen worden
                var benchmarkVragen = _vragen
                    .Where(v => v != null)
                    .Where(v => v.Type != "info" && !string.IsNullOrEmpty(v.Text))
                    .ToList();
                
                // Kosten indicatoren toevoegen
                foreach (var kostType in _kostTypes)
                {
                    if (kostType == null) continue;
                    
                    // Zoek een categorie waar dit kosttype relevant voor is
                    var categorie = _categorieën.FirstOrDefault(c => 
                        c != null && 
                        !string.IsNullOrEmpty(c.RelevantCostTypes) && 
                        c.RelevantCostTypes.Split(',').Select(s => s.Trim()).Contains(kostType.Type));
                    
                    string categorieNaam = categorie != null ? categorie.Text : "Algemeen";
                    
                    // Maak een checkbox voor de kosten indicator
                    CheckBox chk = new CheckBox
                    {
                        Content = $"{categorieNaam} - {kostType.Text}",
                        Tag = $"cost_{kostType.Type}",
                        Margin = new Thickness(0, 2, 0, 2),
                        IsChecked = false // Standaard niet geselecteerd
                    };
                    
                    var listItem = new ListBoxItem { Content = chk };
                    lstKostenIndicatoren.Items.Add(listItem);
                }
                
                // Vragen indicatoren toevoegen
                foreach (var vraag in benchmarkVragen)
                {
                    if (vraag == null) continue;
                    
                    var categorie = _categorieën.FirstOrDefault(c => c != null && c.Nr == vraag.CategoryNr);
                    string categorieNaam = categorie != null ? categorie.Text : "Algemeen";
                    
                    // Maak een checkbox voor de vraag indicator
                    CheckBox chk = new CheckBox
                    {
                        Content = $"{categorieNaam} - {vraag.Text}",
                        Tag = $"question_{vraag.Id}",
                        Margin = new Thickness(0, 2, 0, 2),
                        IsChecked = false // Standaard niet geselecteerd
                    };
                    
                    var listItem = new ListBoxItem { Content = chk };
                    lstVragenIndicatoren.Items.Add(listItem);
                }
                
                // Selecteer standaard de eerste 3 kosten indicatoren (als er genoeg zijn)
                for (int i = 0; i < Math.Min(3, lstKostenIndicatoren.Items.Count); i++)
                {
                    var item = lstKostenIndicatoren.Items[i] as ListBoxItem;
                    if (item != null && item.Content is CheckBox chk)
                    {
                        chk.IsChecked = true;
                        lstKostenIndicatoren.SelectedItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij laden indicatoren: {ex.Message}");
            }
        }

        private void BtnToepassen_Click(object sender, RoutedEventArgs e)
        {
            // Controleer of de invoer geldig is
            if (!ValideerInvoer())
                return;
            
            // Genereer benchmark
            GenereerBenchmark();
        }
        
        private bool ValideerInvoer()
        {
            // Haal geselecteerde jaar en nace-code op
            if (cmbJaar.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een jaar.", "Validatiefout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            // Controleer of er indicatoren geselecteerd zijn
            bool heeftIndicatoren = false;
            
            // Controleer kosten indicatoren
            foreach (ListBoxItem item in lstKostenIndicatoren.Items)
            {
                CheckBox checkBox = item.Content as CheckBox;
                if (checkBox != null && checkBox.IsChecked == true)
                {
                    heeftIndicatoren = true;
                    break;
                }
            }
            
            // Als geen kosten indicatoren, controleer vraag indicatoren
            if (!heeftIndicatoren)
            {
                foreach (ListBoxItem item in lstVragenIndicatoren.Items)
                {
                    CheckBox checkBox = item.Content as CheckBox;
                    if (checkBox != null && checkBox.IsChecked == true)
                    {
                        heeftIndicatoren = true;
                        break;
                    }
                }
            }
            
            if (!heeftIndicatoren)
            {
                MessageBox.Show("Selecteer ten minste één indicator voor de benchmark.", 
                               "Validatiefout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            return true;
        }
        
        private void GenereerBenchmark()
        {
            try
            {
                // Toon een tijdelijke "bezig" melding tijdens het genereren
                spnResultaten.Children.Clear();
                TextBlock tbBezig = new TextBlock
                {
                    Text = "Benchmark genereren, even geduld...",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                spnResultaten.Children.Add(tbBezig);
                
                // Haal filter parameters op
                int jaar = Convert.ToInt32(cmbJaar.SelectedItem);
                
                // NACE code
                string naceFilter = null;
                if (cmbNaceCode.SelectedIndex > 0) // Als niet "Alle" is geselecteerd
                {
                    ComboBoxItem selectedItem = cmbNaceCode.SelectedItem as ComboBoxItem;
                    naceFilter = selectedItem?.Tag as string;
                }
                
                // NACE groupering niveau
                NaceGroupingLevel naceGroepering = NaceGroupingLevel.Niveau2Cijfers; // Standaard niveau 2
                if (rbNiveau3.IsChecked == true)
                    naceGroepering = NaceGroupingLevel.Niveau3Cijfers;
                else if (rbNiveau4of5.IsChecked == true)
                    naceGroepering = NaceGroupingLevel.Niveau4of5Cijfers;
                
                // FTE bereik
                int fteMin = 0;
                int fteMax = int.MaxValue;
                
                if (!string.IsNullOrEmpty(txtFTEMin.Text) && int.TryParse(txtFTEMin.Text, out int min))
                    fteMin = min;
                
                if (!string.IsNullOrEmpty(txtFTEMax.Text) && int.TryParse(txtFTEMax.Text, out int max))
                    fteMax = max;
                
                // Verzamel geselecteerde indicatoren
                List<string> geselecteerdeIndicatoren = new List<string>();
                
                // Kosten indicatoren
                foreach (ListBoxItem item in lstKostenIndicatoren.Items)
                {
                    CheckBox checkBox = item.Content as CheckBox;
                    if (checkBox != null && checkBox.IsChecked == true)
                    {
                        string kostTypeId = checkBox.Tag.ToString();
                        geselecteerdeIndicatoren.Add(kostTypeId);
                    }
                }
                
                // Vraag indicatoren
                foreach (ListBoxItem item in lstVragenIndicatoren.Items)
                {
                    CheckBox checkBox = item.Content as CheckBox;
                    if (checkBox != null && checkBox.IsChecked == true)
                    {
                        string vraagId = checkBox.Tag.ToString();
                        geselecteerdeIndicatoren.Add(vraagId);
                    }
                }
                
                // Haal benchmark data op (aangepast aan de correcte parameter lijst)
                var benchmarkResultaat = _benchmarkService.GetBenchmarkData(
                    _huidigBedrijf.Id, 
                    jaar, 
                    naceFilter, 
                    naceGroepering, 
                    geselecteerdeIndicatoren);
                
                // Sla resultaat op voor later gebruik (bij wijzigen grafiek)
                _huidigBenchmarkResultaat = benchmarkResultaat;
                
                // Update de resultaten tabel
                UpdateResultatenTabel(benchmarkResultaat);
                
                // Update de grafiek
                string grafiekType = (cmbGrafiekType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Staafdiagram";
                UpdateGrafiek(benchmarkResultaat, grafiekType);
                
                // Update het rapport met sterke/zwakke punten
                MaakBenchmarkRapport(benchmarkResultaat);
            }
            catch (Exception ex)
            {
                // Bij fout de foutmelding tonen in de resultaten
                spnResultaten.Children.Clear();
                TextBlock tbFout = new TextBlock
                {
                    Text = $"Fout bij genereren benchmark: {ex.Message}",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Red,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                spnResultaten.Children.Add(tbFout);
                
                // Log voor ontwikkeldoeleinden
                System.Diagnostics.Debug.WriteLine($"Benchmark fout: {ex}");
            }
        }

        private void UpdateResultatenTabel(BenchmarkResultaat benchmarkResultaat)
        {
            try
            {
                // Wis huidige resultaten
                spnResultaten.Children.Clear();
                
                // Verzamel unieke indicatoren
                var indicators = benchmarkResultaat.EigenGegevens
                    .Select(d => d.Indicator)
                    .Distinct()
                    .ToList();
                
                // Header rij toevoegen
                Grid headerRow = new Grid();
                headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
                headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                
                var txtHeaderIndicator = new TextBlock
                {
                    Text = "Indicator",
                    Padding = new Thickness(5),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(txtHeaderIndicator, 0);
                headerRow.Children.Add(txtHeaderIndicator);
                
                var txtHeaderEigen = new TextBlock
                {
                    Text = "Uw waarde",
                    Padding = new Thickness(5),
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(txtHeaderEigen, 1);
                headerRow.Children.Add(txtHeaderEigen);
                
                var txtHeaderGemiddelde = new TextBlock
                {
                    Text = "Gemiddelde",
                    Padding = new Thickness(5),
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(txtHeaderGemiddelde, 2);
                headerRow.Children.Add(txtHeaderGemiddelde);
                
                var txtHeaderMediaan = new TextBlock
                {
                    Text = "Mediaan",
                    Padding = new Thickness(5),
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(txtHeaderMediaan, 3);
                headerRow.Children.Add(txtHeaderMediaan);
                
                var txtHeaderVerschil = new TextBlock
                {
                    Text = "Verschil",
                    Padding = new Thickness(5),
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(txtHeaderVerschil, 4);
                headerRow.Children.Add(txtHeaderVerschil);
                
                spnResultaten.Children.Add(headerRow);
                spnResultaten.Children.Add(new System.Windows.Controls.Separator { Margin = new Thickness(0, 5, 0, 10) });
                
                // Genereer resultaattabel voor elke indicator
                foreach (var indicator in indicators)
                {
                    // Bereken statistieken voor deze indicator
                    var stats = _benchmarkService.BerekenStatistieken(benchmarkResultaat.BenchmarkGegevens, indicator);
                    
                    if (!stats.Any())
                    {
                        continue; // Geen benchmark data voor deze indicator
                    }
                    
                    // Haal eigen waarde
                    var eigenData = benchmarkResultaat.EigenGegevens.FirstOrDefault(d => d.Indicator == indicator);
                    if (eigenData == null)
                    {
                        continue; // Geen eigen data voor deze indicator
                    }
                    
                    // Bepaal naam voor weergave
                    string displayName = GetIndicatorDisplayName(indicator);
                    
                    // Bereken verschilpercentage
                    decimal gemiddelde = stats["Gemiddelde"];
                    decimal eigenWaarde = eigenData.Waarde;
                    decimal mediaan = stats["Mediaan"];
                    
                    decimal verschilPercentage = 0;
                    if (gemiddelde != 0)
                    {
                        verschilPercentage = (eigenWaarde - gemiddelde) / gemiddelde * 100;
                    }
                    
                    string verschilStr = verschilPercentage.ToString("F1") + "%";
                    
                    // Rij voor het resultaat
                    Grid rij = new Grid();
                    rij.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
                    rij.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                    rij.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                    rij.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                    rij.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                    
                    // Indicator naam
                    var txtIndicator = new TextBlock
                    {
                        Text = displayName,
                        Padding = new Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                    };
                    Grid.SetColumn(txtIndicator, 0);
                    rij.Children.Add(txtIndicator);
                    
                    // Eigen waarde
                    var txtEigenWaarde = new TextBlock
                    {
                        Text = FormatteerWaarde(eigenWaarde),
                        Padding = new Thickness(5),
                        TextAlignment = TextAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    Grid.SetColumn(txtEigenWaarde, 1);
                    rij.Children.Add(txtEigenWaarde);
                    
                    // Gemiddelde waarde
                    var txtGemiddelde = new TextBlock
                    {
                        Text = FormatteerWaarde(gemiddelde),
                        Padding = new Thickness(5),
                        TextAlignment = TextAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    Grid.SetColumn(txtGemiddelde, 2);
                    rij.Children.Add(txtGemiddelde);
                    
                    // Mediaan waarde
                    var txtMediaan = new TextBlock
                    {
                        Text = FormatteerWaarde(mediaan),
                        Padding = new Thickness(5),
                        TextAlignment = TextAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    Grid.SetColumn(txtMediaan, 3);
                    rij.Children.Add(txtMediaan);
                    
                    // Verschil
                    var txtVerschil = new TextBlock
                    {
                        Text = verschilStr,
                        Padding = new Thickness(5),
                        TextAlignment = TextAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    
                    // Bepaal kleur op basis van indicator type (kosten of vraag)
                    bool isKostIndicator = indicator.StartsWith("cost_");
                    
                    // Voor kosten is lager beter, voor vragen is hoger beter
                    if (isKostIndicator)
                    {
                        txtVerschil.Foreground = verschilPercentage < 0 ? Brushes.Green : 
                                                (verschilPercentage > 0 ? Brushes.Red : Brushes.Black);
                    }
                    else
                    {
                        txtVerschil.Foreground = verschilPercentage > 0 ? Brushes.Green : 
                                                (verschilPercentage < 0 ? Brushes.Red : Brushes.Black);
                    }
                    
                    Grid.SetColumn(txtVerschil, 4);
                    rij.Children.Add(txtVerschil);
                    
                    // Voeg rij toe aan panel
                    spnResultaten.Children.Add(rij);
                    
                    // Separator toevoegen
                    if (indicators.IndexOf(indicator) < indicators.Count - 1)
                    {
                        spnResultaten.Children.Add(new System.Windows.Controls.Separator { Margin = new Thickness(0, 5, 0, 5) });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij bijwerken resultaten tabel: {ex.Message}");
                MessageBox.Show($"Fout bij bijwerken resultaten: {ex.Message}", 
                               "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetIndicatorDisplayName(string indicator)
        {
            try
            {
                // Voor kosten indicatoren
                if (indicator.StartsWith("cost_"))
                {
                    string kostTypeCode = indicator.Substring(5);
                    var kostType = _kostTypes.FirstOrDefault(kt => kt.Type == kostTypeCode);
                    
                    if (kostType != null)
                    {
                        return kostType.Text;
                    }
                    
                    return $"Kosten: {kostTypeCode}";
                }
                
                // Voor vraag indicatoren
                if (indicator.StartsWith("question_"))
                {
                    string vraagIdStr = indicator.Substring(9);
                    if (int.TryParse(vraagIdStr, out int vraagId))
                    {
                        var vraag = _vragen.FirstOrDefault(v => v.Id == vraagId);
                        
                        if (vraag != null)
                        {
                            return vraag.Text;
                        }
                    }
                    
                    return $"Vraag {vraagIdStr}";
                }
                
                return indicator;
            }
            catch
            {
                return indicator;
            }
        }

        private string FormatteerWaarde(decimal waarde)
        {
            // Formatteert de waarde afhankelijk van grote getallen
            if (Math.Abs(waarde) >= 1000)
            {
                return $"€ {waarde:N0}";
            }
            else
            {
                return waarde.ToString("N2");
            }
        }

        private void UpdateGrafiek(BenchmarkResultaat benchmarkResultaat, string grafiekType)
        {
            // Verberg placeholder en toon grafiek
            tbGrafiekPlaceholder.Visibility = Visibility.Collapsed;
            grafiekControl.Visibility = Visibility.Visible;
            
            // Reset de grafiek
            grafiekControl.Series.Clear();
            grafiekControl.AxisX.Clear();
            grafiekControl.AxisY.Clear();
            
            // Verberg box plot selector (niet meer gebruikt)
            
            // Update de grafiek op basis van het geselecteerde type
            switch (grafiekType.ToLower())
            {
                case "staafdiagram":
                    TekenStaafdiagram(benchmarkResultaat);
                    break;
                case "lijndiagram":
                    TekenLijndiagram(benchmarkResultaat);
                    break;
                default:
                    TekenStaafdiagram(benchmarkResultaat); // Standaard: staafdiagram
                    break;
            }
        }

        private void TekenStaafdiagram(BenchmarkResultaat benchmarkResultaat)
        {
            try
            {
                // Verzamel de data voor visualisatie
                var indicators = benchmarkResultaat.EigenGegevens
                    .Select(d => d.Indicator)
                    .Distinct()
                    .ToList();
                
                // Verzamel de data
                ChartValues<double> eigenWaarden = new ChartValues<double>();
                ChartValues<double> gemiddeldeWaarden = new ChartValues<double>();
                List<string> labels = new List<string>();
                
                foreach (var indicator in indicators)
                {
                    // Eigen waarde
                    var eigenItem = benchmarkResultaat.EigenGegevens.FirstOrDefault(d => d.Indicator == indicator);
                    if (eigenItem != null)
                    {
                        eigenWaarden.Add((double)eigenItem.Waarde);
                        
                        // Gebruik de volledige naam van de indicator (geen afkorting meer)
                        string displayName = GetIndicatorDisplayName(indicator);
                        // Splits het label in meerdere regels voor betere verticale weergave
                        displayName = SplitLabelIntoLines(displayName, 2); // Aangepast naar 2 woorden per regel
                        labels.Add(displayName);
                    }
                    
                    // Marktgemiddelde
                    var stats = _benchmarkService.BerekenStatistieken(benchmarkResultaat.BenchmarkGegevens, indicator);
                    if (stats.ContainsKey("Gemiddelde"))
                    {
                        gemiddeldeWaarden.Add((double)stats["Gemiddelde"]);
                    }
                }
                
                // Als er geen data is, toon een melding
                if (eigenWaarden.Count == 0 || gemiddeldeWaarden.Count == 0)
                {
                    tbGrafiekPlaceholder.Text = "Geen gegevens beschikbaar voor de geselecteerde criteria";
                    tbGrafiekPlaceholder.Visibility = Visibility.Visible;
                    grafiekControl.Visibility = Visibility.Collapsed;
                    return;
                }
                
                // Configureer de series
                var eigenSerie = new ColumnSeries
                {
                    Title = "Uw bedrijf",
                    Values = eigenWaarden,
                    Fill = new SolidColorBrush(Color.FromRgb(33, 150, 243)),  // Mooie blauwe kleur
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("N0"),
                    MaxColumnWidth = 40, // Smallere kolommen
                    ColumnPadding = 20 // Meer padding tussen kolommen
                };
                
                var gemiddeldeSerie = new ColumnSeries
                {
                    Title = "Marktgemiddelde",
                    Values = gemiddeldeWaarden,
                    Fill = new SolidColorBrush(Color.FromRgb(66, 66, 66)),    // Donkergrijze kleur
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("N0"),
                    MaxColumnWidth = 40, // Smallere kolommen
                    ColumnPadding = 20 // Meer padding tussen kolommen
                };
                
                // Voeg series toe aan de grafiek
                grafiekControl.Series = new SeriesCollection { eigenSerie, gemiddeldeSerie };
                
                // Configureer X-as (categorieën)
                grafiekControl.AxisX.Clear();
                grafiekControl.AxisX.Add(new Axis
                {
                    Title = "Indicators",
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator { Step = 1 },
                    
                    // Labels horizontaal houden voor betere leesbaarheid
                    LabelsRotation = 0,
                    
                    // Verhoog de tekstgrootte voor betere leesbaarheid
                    FontSize = 11
                });
                
                // Configureer Y-as (waarden)
                grafiekControl.AxisY.Clear();
                grafiekControl.AxisY.Add(new Axis
                {
                    Title = "Waarde",
                    LabelFormatter = value => value.ToString("N0"),
                    MinValue = 0
                });
                
                // Configureer de legenda
                grafiekControl.LegendLocation = LegendLocation.Top;
                
                // Maximaliseer de grafiek grootte en maak voldoende ruimte voor horizontale labels
                grafiekControl.Height = 300;
                grafiekControl.Width = double.NaN; // Auto width
                grafiekControl.DisableAnimations = false;
                grafiekControl.AnimationsSpeed = TimeSpan.FromMilliseconds(500);
                grafiekControl.Margin = new Thickness(20, 20, 20, 70); // Extra ruimte voor tweeregelige labels
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij maken staafdiagram: {ex.Message}", 
                               "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TekenLijndiagram(BenchmarkResultaat benchmarkResultaat)
        {
            try
            {
                // Verzamel de data voor visualisatie
                var indicators = benchmarkResultaat.EigenGegevens
                    .Select(d => d.Indicator)
                    .Distinct()
                    .ToList();
                
                // Verzamel de data
                ChartValues<double> eigenWaarden = new ChartValues<double>();
                ChartValues<double> gemiddeldeWaarden = new ChartValues<double>();
                List<string> labels = new List<string>();
                
                foreach (var indicator in indicators)
                {
                    // Eigen waarde
                    var eigenItem = benchmarkResultaat.EigenGegevens.FirstOrDefault(d => d.Indicator == indicator);
                    if (eigenItem != null)
                    {
                        eigenWaarden.Add((double)eigenItem.Waarde);
                        
                        // Gebruik de volledige naam van de indicator (geen afkorting meer)
                        string displayName = GetIndicatorDisplayName(indicator);
                        // Splits het label in meerdere regels voor betere verticale weergave
                        displayName = SplitLabelIntoLines(displayName, 2); // Aangepast naar 2 woorden per regel
                        labels.Add(displayName);
                    }
                    
                    // Marktgemiddelde
                    var stats = _benchmarkService.BerekenStatistieken(benchmarkResultaat.BenchmarkGegevens, indicator);
                    if (stats.ContainsKey("Gemiddelde"))
                    {
                        gemiddeldeWaarden.Add((double)stats["Gemiddelde"]);
                    }
                }
                
                // Als er geen data is, toon een melding
                if (eigenWaarden.Count == 0 || gemiddeldeWaarden.Count == 0)
                {
                    tbGrafiekPlaceholder.Text = "Geen gegevens beschikbaar voor de geselecteerde criteria";
                    tbGrafiekPlaceholder.Visibility = Visibility.Visible;
                    grafiekControl.Visibility = Visibility.Collapsed;
                    return;
                }
                
                // Configureer de series
                var eigenSerie = new LineSeries
                {
                    Title = "Uw bedrijf",
                    Values = eigenWaarden,
                    Stroke = new SolidColorBrush(Color.FromRgb(33, 150, 243)),  // Mooie blauwe kleur
                    PointGeometry = DefaultGeometries.Diamond,
                    PointGeometrySize = 15,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("N0"),
                    LineSmoothness = 0   // Rechte lijnen tussen punten
                };
                
                var gemiddeldeSerie = new LineSeries
                {
                    Title = "Marktgemiddelde",
                    Values = gemiddeldeWaarden,
                    Stroke = new SolidColorBrush(Color.FromRgb(66, 66, 66)),    // Donkergrijze kleur
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 15,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("N0"),
                    LineSmoothness = 0   // Rechte lijnen tussen punten
                };
                
                // Voeg series toe aan de grafiek
                grafiekControl.Series = new SeriesCollection { eigenSerie, gemiddeldeSerie };
                
                // Configureer X-as (categorieën)
                grafiekControl.AxisX.Clear();
                grafiekControl.AxisX.Add(new Axis
                {
                    Title = "Indicators",
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator { Step = 1 },
                    
                    // Labels horizontaal houden voor betere leesbaarheid
                    LabelsRotation = 0,
                    
                    // Verhoog de tekstgrootte voor betere leesbaarheid
                    FontSize = 11
                });
                
                // Configureer Y-as (waarden)
                grafiekControl.AxisY.Clear();
                grafiekControl.AxisY.Add(new Axis
                {
                    Title = "Waarde",
                    LabelFormatter = value => value.ToString("N0"),
                    MinValue = 0
                });
                
                // Configureer de legenda
                grafiekControl.LegendLocation = LegendLocation.Top;
                
                // Maximaliseer de grafiek grootte en maak voldoende ruimte voor horizontale labels
                grafiekControl.Height = 300;
                grafiekControl.Width = double.NaN; // Auto width
                grafiekControl.DisableAnimations = false;
                grafiekControl.AnimationsSpeed = TimeSpan.FromMilliseconds(500);
                grafiekControl.Margin = new Thickness(20, 20, 20, 70); // Extra ruimte voor tweeregelige labels
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij maken lijndiagram: {ex.Message}", 
                               "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string SplitLabelIntoLines(string label, int maxCharPerLine = 12)
        {
            // Controleer op lege input
            if (string.IsNullOrEmpty(label))
                return label;

            // Voor korte labels geen wijzigingen
            if (label.Length <= maxCharPerLine)
                return label;
            
            // Verdeel in woorden
            string[] woorden = label.Split(' ');
            
            // Als er maar één woord is, verdeel het in stukken
            if (woorden.Length == 1)
            {
                int mid = label.Length / 2;
                return label.Substring(0, mid) + Environment.NewLine + label.Substring(mid);
            }
            
            // Verdeel de woorden over meerdere regels
            StringBuilder result = new StringBuilder();
            int huidigeRegelLengte = 0;
            
            for (int i = 0; i < woorden.Length; i++)
            {
                string woord = woorden[i];
                
                // Als dit woord de regel te lang maakt, begin een nieuwe regel
                if (huidigeRegelLengte > 0 && huidigeRegelLengte + woord.Length > maxCharPerLine)
                {
                    result.Append(Environment.NewLine);
                    huidigeRegelLengte = 0;
                }
                
                // Voeg het woord toe
                result.Append(woord);
                huidigeRegelLengte += woord.Length;
                
                // Voeg spatie toe als het niet het laatste woord is
                if (i < woorden.Length - 1)
                {
                    result.Append(" ");
                    huidigeRegelLengte += 1;
                }
            }
            
            return result.ToString();
        }

        private void BtnTerug_Click(object sender, RoutedEventArgs e)
        {
            // Navigeer terug naar dashboard
            if (_hoofdVenster != null)
            {
                _hoofdVenster.NavigeerNaar(new CompanyDashboardPage());
            }
        }
        
        // Alias methode voor de nieuwe knopnaam
        private void BtnTerugNaarDashboard_Click(object sender, RoutedEventArgs e)
        {
            BtnTerug_Click(sender, e);
        }
        
        /// <summary>
        /// Verwerkt het rapport en visualiseert het in verschillende secties op basis van sterke/zwakke punten
        /// </summary>
        /// <param name="rapportTekst">Tekstueel rapport van de benchmark service</param>
        /// <param name="benchmarkResultaat">Benchmark resultaat data</param>
        private void MaakBenchmarkRapport(BenchmarkResultaat benchmarkResultaat)
        {
            try
            {
                // Wis bestaande inhoud
                spnSterkePuntenInhoud.Children.Clear();
                spnZwakkePuntenInhoud.Children.Clear();
                spnNeutralePuntenInhoud.Children.Clear();

                // Maak secties zichtbaar/onzichtbaar afhankelijk van inhoud
                spnSterkePunten.Visibility = Visibility.Collapsed;
                spnZwakkePunten.Visibility = Visibility.Collapsed;
                spnNeutralePunten.Visibility = Visibility.Collapsed;
                
                // Initialiseer tellers voor dashboard weergave
                int sterkePuntenCount = 0;
                int zwakkePuntenCount = 0;
                int neutralePuntenCount = 0;
                
                // Intro bericht maken
                tbRapportIntro.Text = $"Benchmark rapport gegenereerd op basis van data van andere bedrijven " +
                                    $"in {benchmarkResultaat.BenchmarkGegevens.Select(bg => bg.NaceCode).Distinct().Count()} " +
                                    $"sector(en) voor het jaar {benchmarkResultaat.BenchmarkGegevens.FirstOrDefault()?.Jaar ?? DateTime.Now.Year}.";
                
                // Genereer tekstueel rapport voor het benchmarkresultaat
                string rapportTekst = _benchmarkService.GenereerRapport(benchmarkResultaat);
                
                // Controleer of er data is
                if (string.IsNullOrEmpty(rapportTekst) || rapportTekst.Contains("Er zijn onvoldoende gegevens"))
                {
                    tbRapportIntro.Text = "Er zijn onvoldoende gegevens beschikbaar om een rapport te genereren.";
                    return;
                }
                
                // Rapport opdelen in secties per indicator
                string[] secties = rapportTekst.Split(new[] { "-------------------------------------------" }, 
                                                     StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string sectie in secties)
                {
                    // Filter lege secties en headers/footers
                    if (string.IsNullOrWhiteSpace(sectie) || 
                        sectie.Contains("BENCHMARK RAPPORT") ||
                        sectie.Contains("===================") ||
                        sectie.Contains("EINDE RAPPORT"))
                    {
                        continue;
                    }
                    
                    // Bepaal indicator naam
                    string indicatorNaam = string.Empty;
                    if (sectie.Contains("INDICATOR:"))
                    {
                        var regels = sectie.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var regel in regels)
                        {
                            if (regel.StartsWith("INDICATOR:"))
                            {
                                indicatorNaam = regel.Replace("INDICATOR:", "").Trim();
                                break;
                            }
                        }
                    }
                    
                    // Als er geen naam is gevonden, sla over
                    if (string.IsNullOrEmpty(indicatorNaam))
                    {
                        continue;
                    }
                    
                    // Bepaal waarden
                    decimal eigenWaarde = 0;
                    decimal gemiddeldeWaarde = 0;
                    decimal mediaanWaarde = 0;
                    
                    var rapportRegels = sectie.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var regel in rapportRegels)
                    {
                        if (regel.StartsWith("Eigen waarde:"))
                        {
                            var waardeStr = regel.Replace("Eigen waarde:", "").Trim();
                            decimal.TryParse(waardeStr, out eigenWaarde);
                        }
                        else if (regel.StartsWith("Benchmark gemiddelde:"))
                        {
                            var waardeStr = regel.Replace("Benchmark gemiddelde:", "").Trim();
                            decimal.TryParse(waardeStr, out gemiddeldeWaarde);
                        }
                        else if (regel.StartsWith("Benchmark mediaan:"))
                        {
                            var waardeStr = regel.Replace("Benchmark mediaan:", "").Trim();
                            decimal.TryParse(waardeStr, out mediaanWaarde);
                        }
                    }
                    
                    // Bepaal of het een sterk/zwak/neutraal punt is
                    StackPanel doelPanel;
                    Brush kleur;
                    
                    if (sectie.Contains("STERK PUNT:"))
                    {
                        doelPanel = spnSterkePuntenInhoud;
                        kleur = Brushes.DarkGreen;
                        sterkePuntenCount++;
                        spnSterkePunten.Visibility = Visibility.Visible;
                    }
                    else if (sectie.Contains("AANDACHTSPUNT:"))
                    {
                        doelPanel = spnZwakkePuntenInhoud;
                        kleur = Brushes.Firebrick;
                        zwakkePuntenCount++;
                        spnZwakkePunten.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        doelPanel = spnNeutralePuntenInhoud;
                        kleur = Brushes.DarkGray;
                        neutralePuntenCount++;
                        spnNeutralePunten.Visibility = Visibility.Visible;
                    }
                    
                    // Maak een formatted indicator panel
                    var indicatorPanel = new StackPanel
                    {
                        Margin = new Thickness(0, 5, 0, 10)
                    };
                    
                    // Indicator titel
                    var titelTextBlock = new TextBlock
                    {
                        Text = indicatorNaam,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = kleur,
                        Margin = new Thickness(0, 0, 0, 5),
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                    indicatorPanel.Children.Add(titelTextBlock);
                    
                    // Gebruik een compacter grid layout voor de waardenweergave
                    var waardenGrid = new Grid();
                    waardenGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    waardenGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    
                    // Maak 3 rijen voor de waarden
                    for (int i = 0; i < 3; i++)
                    {
                        waardenGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }
                    
                    // Tooltip beschrijvingen voor betere UI/UX
                    // Eigen waarde rij
                    var lblEigen = new TextBlock
                    {
                        Text = "Uw waarde:",
                        FontWeight = FontWeights.Normal,
                        Margin = new Thickness(0, 2, 10, 2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(lblEigen, 0);
                    Grid.SetColumn(lblEigen, 0);
                    waardenGrid.Children.Add(lblEigen);
                    
                    var txtEigen = new TextBlock
                    {
                        Text = FormatteerWaarde(eigenWaarde),
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 2, 0, 2),
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(txtEigen, 0);
                    Grid.SetColumn(txtEigen, 1);
                    waardenGrid.Children.Add(txtEigen);
                    
                    // Gemiddelde rij
                    var lblGemiddelde = new TextBlock
                    {
                        Text = "Gemiddelde:",
                        FontWeight = FontWeights.Normal,
                        Margin = new Thickness(0, 2, 10, 2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(lblGemiddelde, 1);
                    Grid.SetColumn(lblGemiddelde, 0);
                    waardenGrid.Children.Add(lblGemiddelde);
                    
                    var txtGemiddelde = new TextBlock
                    {
                        Text = FormatteerWaarde(gemiddeldeWaarde),
                        Margin = new Thickness(0, 2, 0, 2),
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(txtGemiddelde, 1);
                    Grid.SetColumn(txtGemiddelde, 1);
                    waardenGrid.Children.Add(txtGemiddelde);
                    
                    // Mediaan rij
                    var lblMediaan = new TextBlock
                    {
                        Text = "Mediaan:",
                        FontWeight = FontWeights.Normal,
                        Margin = new Thickness(0, 2, 10, 2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(lblMediaan, 2);
                    Grid.SetColumn(lblMediaan, 0);
                    waardenGrid.Children.Add(lblMediaan);
                    
                    var txtMediaan = new TextBlock
                    {
                        Text = FormatteerWaarde(mediaanWaarde),
                        Margin = new Thickness(0, 2, 0, 2),
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(txtMediaan, 2);
                    Grid.SetColumn(txtMediaan, 1);
                    waardenGrid.Children.Add(txtMediaan);
                    
                    indicatorPanel.Children.Add(waardenGrid);
                    
                    // Analyse tekst
                    string analyseText = string.Empty;
                    foreach (var regel in rapportRegels)
                    {
                        if (regel.Contains("STERK PUNT:"))
                        {
                            analyseText = regel.Replace("STERK PUNT:", "").Trim();
                            break;
                        }
                        else if (regel.Contains("AANDACHTSPUNT:"))
                        {
                            analyseText = regel.Replace("AANDACHTSPUNT:", "").Trim();
                            break;
                        }
                        else if (regel.Contains("ligt dicht bij het benchmark gemiddelde"))
                        {
                            analyseText = regel.Trim();
                            break;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(analyseText))
                    {
                        var analyseTxtBlock = new TextBlock
                        {
                            Text = analyseText,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 8, 0, 0),
                            FontStyle = FontStyles.Italic,
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };
                        indicatorPanel.Children.Add(analyseTxtBlock);
                    }
                    
                    // Voeg separator toe
                    if (doelPanel.Children.Count > 0)
                    {
                        doelPanel.Children.Add(new System.Windows.Controls.Separator { Margin = new Thickness(0, 5, 0, 5) });
                    }
                    
                    // Voeg toe aan de juiste sectie
                    doelPanel.Children.Add(indicatorPanel);
                }
                
                // Als er geen sterke punten zijn, toon een bericht
                if (sterkePuntenCount == 0 && spnSterkePunten.Visibility == Visibility.Visible)
                {
                    var txtGeenSterkePunten = new TextBlock
                    {
                        Text = "Er zijn geen significante sterke punten geïdentificeerd in de benchmark.",
                        TextWrapping = TextWrapping.Wrap,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(5)
                    };
                    spnSterkePuntenInhoud.Children.Add(txtGeenSterkePunten);
                }
                
                // Als er geen zwakke punten zijn, toon een bericht
                if (zwakkePuntenCount == 0 && spnZwakkePunten.Visibility == Visibility.Visible)
                {
                    var txtGeenZwakkePunten = new TextBlock
                    {
                        Text = "Er zijn geen significante aandachtspunten geïdentificeerd in de benchmark.",
                        TextWrapping = TextWrapping.Wrap,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(5)
                    };
                    spnZwakkePuntenInhoud.Children.Add(txtGeenZwakkePunten);
                }
                
                // Als er geen neutrale punten zijn, toon een bericht
                if (neutralePuntenCount == 0 && spnNeutralePunten.Visibility == Visibility.Visible)
                {
                    var txtGeenNeutralePunten = new TextBlock
                    {
                        Text = "Er zijn geen neutrale indicatoren geïdentificeerd in de benchmark.",
                        TextWrapping = TextWrapping.Wrap,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(5)
                    };
                    spnNeutralePuntenInhoud.Children.Add(txtGeenNeutralePunten);
                }
            }
            catch (Exception ex)
            {
                tbRapportIntro.Text = $"Fout bij verwerken van rapport: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Fout bij verwerken van rapport: {ex.Message}");
            }
        }
    }
} 