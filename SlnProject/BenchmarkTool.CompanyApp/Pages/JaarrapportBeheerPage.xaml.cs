using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BenchmarkTool.ClassLibrary.Models;
using BenchmarkTool.ClassLibrary.Services;

namespace BenchmarkTool.CompanyApp.Pages
{
    /// <summary>
    /// Beheerpagina voor jaarrapporten
    /// </summary>
    public partial class JaarrapportBeheerPage : Page
    {
        // Huidige geselecteerd rapport
        private Button _geselecteerdRapportButton;
        private int? _geselecteerdRapportId;
        // Referentie naar het hoofdvenster
        private CompanyMainWindow _hoofdVenster;
        
        // Services
        private readonly JaarrapportService _jaarrapportService;
        private readonly BedrijfService _bedrijfService;
        
        // Data containers
        private List<Categorie> _categorieën;
        private List<KostType> _kostTypes;
        private List<Vraag> _vragen;
        
        // Dictionary voor het bijhouden van invoervelden per kost/vraag
        private Dictionary<string, TextBox> _invoerVelden = new Dictionary<string, TextBox>();

        public JaarrapportBeheerPage()
        {
            try
        {
            InitializeComponent();
                
                // Haal het hoofdvenster op
                _hoofdVenster = (CompanyMainWindow)Application.Current.MainWindow;
                
                // Initialiseer services
                _jaarrapportService = new JaarrapportService();
                _bedrijfService = new BedrijfService();
                
                // Voeg een terugknop toe aan de pagina
                VoegTerugknopToe();
                
                // Laad stamgegevens (categorieën, kosttypes, vragen)
                LaadStamgegevens();
                
                // Laad jaarrapporten van het ingelogde bedrijf
                LaadJaarrapporten();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij initialiseren van de jaarrapportbeheer pagina: {ex.Message}", 
                                "Initialisatiefout", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Exception in JaarrapportBeheerPage constructor: {ex}");
            }
        }

        private void VoegTerugknopToe()
        {
            try
            {
                // Voeg een terugknop toe bovenaan de pagina
                Button btnTerug = new Button
                {
                    Content = "Terug naar dashboard",
                    Style = (Style)FindResource("StandardButtonStyle"),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                btnTerug.Click += (s, e) => _hoofdVenster.NavigeerNaar(new CompanyDashboardPage());
                
                // Voeg een refresh knop toe
                Button btnRefresh = new Button
                {
                    Content = "Vernieuw pagina",
                    Style = (Style)FindResource("StandardButtonStyle"),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                btnRefresh.Click += (s, e) => VernieuwPagina();
                
                // Maak een panel met beide knoppen
                Grid knoppen = new Grid();
                knoppen.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                knoppen.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                
                Grid.SetColumn(btnTerug, 0);
                Grid.SetColumn(btnRefresh, 1);
                knoppen.Children.Add(btnTerug);
                knoppen.Children.Add(btnRefresh);
                
                // Voeg de knoppen toe aan de pagina
                StackPanel headerPanel = (StackPanel)((Grid)this.Content).Children[0];
                headerPanel.Children.Insert(0, knoppen);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij toevoegen van knoppen: {ex.Message}");
                // Geen verdere actie nodig, de knoppen zijn niet essentieel
            }
        }

        private void VernieuwPagina()
        {
            try
            {
                // Toon een laadmelding
                spnKostenCategorieën.Children.Clear();
                TextBlock txtLaad = new TextBlock
                {
                    Text = "Bezig met verversen van de pagina...",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 20, 0, 20),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                spnKostenCategorieën.Children.Add(txtLaad);
                
                // Forceer UI update
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                    () => { }, 
                    System.Windows.Threading.DispatcherPriority.Background);
                
                try
                {
                    // Herlaad de stamgegevens
                    LaadStamgegevens();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Fout bij herladen stamgegevens: {ex.Message}");
                    // We gaan door ondanks de fout
                }
                
                try
                {
                    // Herlaad de jaarrapporten
            LaadJaarrapporten();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Fout bij herladen jaarrapporten: {ex.Message}");
                    // We gaan door ondanks de fout
                }
                
                // Maak de detailsectie leeg of herlaad details
                if (_geselecteerdRapportId.HasValue)
                {
                    try
                    {
                        LaadRapportDetails(_geselecteerdRapportId.Value);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Fout bij herladen rapportdetails: {ex.Message}");
                        
                        // Toon een foutmelding in de details sectie
                        spnKostenCategorieën.Children.Clear();
                        TextBlock txtError = new TextBlock
                        {
                            Text = $"Fout bij het laden van rapportdetails: {ex.Message}. Klik op 'Vernieuw pagina' om opnieuw te proberen.",
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = Brushes.Red,
                            Margin = new Thickness(0, 10, 0, 10)
                        };
                        spnKostenCategorieën.Children.Add(txtError);
                    }
                }
                else
                {
                    spnKostenCategorieën.Children.Clear();
                    TextBlock txtSelecteer = new TextBlock
                    {
                        Text = "Selecteer een jaarrapport of maak een nieuw rapport aan.",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 20, 0, 20),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    spnKostenCategorieën.Children.Add(txtSelecteer);
                }
            }
            catch (Exception ex)
            {
                // Centrale foutafhandeling voor het geval dat iets anders misgaat
                string bericht = $"Fout bij het vernieuwen van de pagina: {ex.Message}";
                if (ex.InnerException != null)
                {
                    bericht += $"\nInnerException: {ex.InnerException.Message}";
                }
                
                MessageBox.Show(bericht, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Exception in VernieuwPagina: {ex}");
                
                // Reset de pagina tot een minimale staat
                spnKostenCategorieën.Children.Clear();
                TextBlock txtError = new TextBlock
                {
                    Text = "Er is een fout opgetreden bij het vernieuwen van de pagina. Probeer opnieuw in te loggen of de applicatie te herstarten.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Red,
                    Margin = new Thickness(0, 10, 0, 10)
                };
                spnKostenCategorieën.Children.Add(txtError);
            }
        }

        private void LaadStamgegevens()
        {
            try
            {
                // Haal categorieën op
                _categorieën = _bedrijfService.GetAllCategories() ?? new List<Categorie>();
                if (_categorieën.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Waarschuwing: Geen categorieën gevonden in de database.");
                }
                
                // Haal kosttypes op
                _kostTypes = _bedrijfService.GetAllCosttypes() ?? new List<KostType>();
                if (_kostTypes.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Waarschuwing: Geen kosttypes gevonden in de database.");
                }
                
                // Haal vragen op (alleen niet-info vragen)
                var alleVragen = _bedrijfService.GetAllQuestions();
                
                // Zeer veilige filtering om null values te voorkomen
                if (alleVragen != null)
                {
                    // Zeer defensieve filtering - negeer alle null items en controleer zorgvuldig op Type waarden
                    _vragen = alleVragen
                        .Where(v => v != null) // Verwijder null vragen
                        .Where(v => string.IsNullOrEmpty(v.Type) || !v.Type.Equals("info", StringComparison.OrdinalIgnoreCase)) // Negeer info vragen
                        .ToList();
                }
                else
                {
                    _vragen = new List<Vraag>();
                    System.Diagnostics.Debug.WriteLine("Waarschuwing: Geen vragen gevonden in de database.");
                }
            }
            catch (Exception ex)
            {
                // Verbeterde foutafhandeling met meer details
                string bericht = $"Fout bij laden van stamgegevens: {ex.Message}";
                if (ex.InnerException != null)
                {
                    bericht += $"\nInnerException: {ex.InnerException.Message}";
                }
                
                MessageBox.Show(bericht, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Initialiseer lege collecties om NullReferenceExceptions te voorkomen
                _categorieën = new List<Categorie>();
                _kostTypes = new List<KostType>();
                _vragen = new List<Vraag>();
                
                // Log de fout voor debugging
                System.Diagnostics.Debug.WriteLine($"Exception in LaadStamgegevens: {ex}");
            }
        }

        private void LaadJaarrapporten()
        {
            try
            {
                // Wis lijst
                spnJaarrapporten.Children.Clear();
                
                if (App.IngelogdBedrijf == null)
                {
                    MessageBox.Show("U bent niet ingelogd. Log opnieuw in om uw jaarrapporten te bekijken.", 
                                   "Niet ingelogd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _hoofdVenster.NavigeerNaar(new CompanyLoginPage());
                    return;
                }
                
                // Haal jaarrapporten op voor het ingelogde bedrijf
                var jaarrapporten = _jaarrapportService.GetYearreportsByCompany(App.IngelogdBedrijf.Id);
                
                // Controleer of jaarrapporten goed zijn opgehaald
                if (jaarrapporten == null)
                {
                    MessageBox.Show("Er konden geen jaarrapporten worden opgehaald. Probeer het opnieuw.", 
                                    "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Sorteer op jaar (nieuwste eerst)
                jaarrapporten = jaarrapporten.OrderByDescending(j => j.Year).ToList();
                
                if (jaarrapporten.Count == 0)
                {
                    // Voeg een melding toe als er nog geen jaarrapporten zijn
                    TextBlock txtGeenRapporten = new TextBlock
                    {
                        Text = "U heeft nog geen jaarrapporten. Maak een nieuw rapport aan met de knop hieronder.",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 10),
                        Foreground = Brushes.Gray
                    };
                    spnJaarrapporten.Children.Add(txtGeenRapporten);
                }
                else
                {
                    // Voeg elk jaarrapport toe aan de lijst
                    foreach (var rapport in jaarrapporten)
                    {
                        Button btnRapport = new Button
                        {
                            Content = rapport.Year.ToString(),
                            Tag = rapport.Id, // Bewaar het rapport ID
                            Style = (Style)FindResource("StandardButtonStyle"),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        btnRapport.Click += SelectJaarrapport;
                        spnJaarrapporten.Children.Add(btnRapport);
                    }
                }
            }
            catch (Exception ex)
            {
                // Verbeterde foutmelding met meer details
                string bericht = $"Fout bij het laden van jaarrapporten: {ex.Message}";
                if (ex.InnerException != null)
                {
                    bericht += $"\nInnerException: {ex.InnerException.Message}";
                }
                MessageBox.Show(bericht, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Log de fout voor debugging
                System.Diagnostics.Debug.WriteLine($"Exception in LaadJaarrapporten: {ex}");
            }
        }

        private void LaadKostenCategorieen()
        {
            try
            {
                // Wis bestaande categorieën
                spnKostenCategorieën.Children.Clear();
                
                // Controleer of we categorieën en kosttypes hebben geladen
                if (_categorieën == null || _categorieën.Count == 0 || 
                    _kostTypes == null || _kostTypes.Count == 0)
                {
                    TextBlock txtGeenData = new TextBlock
                    {
                        Text = "Kon categorieën of kosttypes niet laden. De server probeert het nu opnieuw...",
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = Brushes.Red,
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    spnKostenCategorieën.Children.Add(txtGeenData);

                    // Probeer de data opnieuw te laden
                    LaadStamgegevens();
                    
                    // Controleer opnieuw of we nu wel data hebben
                    if ((_categorieën == null || _categorieën.Count == 0) &&
                        (_kostTypes == null || _kostTypes.Count == 0))
                    {
                        TextBlock txtNogGeenData = new TextBlock
                        {
                            Text = "Het is niet gelukt om de benodigde data te laden. Probeer de pagina te vernieuwen.",
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = Brushes.Red,
                            Margin = new Thickness(0, 10, 0, 10)
                        };
                        spnKostenCategorieën.Children.Add(txtNogGeenData);
                        
                        Button btnVernieuw = new Button
                        {
                            Content = "Pagina vernieuwen",
                            Style = (Style)FindResource("StandardButtonStyle"),
                            Margin = new Thickness(0, 10, 0, 10)
                        };
                        btnVernieuw.Click += (s, e) => VernieuwPagina();
                        spnKostenCategorieën.Children.Add(btnVernieuw);
                        
                        return;
                    }
                }
                
                // Haal rapport details op als er een geselecteerd rapport is
                Dictionary<string, object> rapportDetails = null;
                Dictionary<string, decimal> kostenWaarden = new Dictionary<string, decimal>();
                Dictionary<string, string> antwoordWaarden = new Dictionary<string, string>();
                
                if (_geselecteerdRapportId.HasValue)
                {
                    try
                    {
                        rapportDetails = _jaarrapportService.GetYearreportDetails(_geselecteerdRapportId.Value);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Fout bij ophalen rapportdetails: {ex.Message}");
                        // Toon een waarschuwing maar ga door
                        TextBlock txtWaarschuwing = new TextBlock
                        {
                            Text = "Waarschuwing: Kon niet alle details van het rapport laden. Sommige velden kunnen leeg zijn.",
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = Brushes.Orange,
                            Margin = new Thickness(0, 0, 0, 10)
                        };
                        spnKostenCategorieën.Children.Add(txtWaarschuwing);
                    }
                    
                    if (rapportDetails != null)
                    {
                        // Verwerk kosten
                        if (rapportDetails.ContainsKey("Kosten") && rapportDetails["Kosten"] != null)
                        {
                            var kosten = (List<Kost>)rapportDetails["Kosten"];
                            foreach (var kost in kosten)
                            {
                                if (kost != null)
                                {
                                    string key = $"cost_{kost.CategoryNr}_{kost.CosttypeType}";
                                    kostenWaarden[key] = kost.Value;
                                }
                            }
                        }
                        
                        // Verwerk antwoorden
                        if (rapportDetails.ContainsKey("Antwoorden") && rapportDetails["Antwoorden"] != null)
                        {
                            var antwoorden = (List<Antwoord>)rapportDetails["Antwoorden"];
                            foreach (var antwoord in antwoorden)
                            {
                                if (antwoord != null)
                                {
                                    string key = $"answer_{antwoord.QuestionId}";
                                    antwoordWaarden[key] = antwoord.Value;
                                }
                            }
                        }
                    }
                }
                
                // Loop door alle categorieën gesorteerd op nummer
                foreach (var categorie in _categorieën.Where(c => c != null).OrderBy(c => c.Nr))
                {
                    if (categorie == null) continue;
                    
                    // Voeg een Expander toe voor elke categorie (kan in- en uitgeklapt worden)
                    Expander expCategorie = new Expander
                    {
                        Header = categorie.Text ?? "Onbekende categorie",
                        ToolTip = categorie.Tooltip,
                        IsExpanded = true, // Standaard uitgeklapt
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    
                    // Content container voor de categorie
                    StackPanel panelContent = new StackPanel { Margin = new Thickness(15, 10, 0, 5) };
                    
                    // Voeg kosttypes toe als die relevant zijn voor deze categorie
                    bool heeftContent = false;
                    if (!string.IsNullOrEmpty(categorie.RelevantCostTypes))
                    {
                        var relevanteCostTypes = categorie.RelevantCostTypes.Split(',');
                        foreach (var kostTypeStr in relevanteCostTypes)
                        {
                            if (string.IsNullOrEmpty(kostTypeStr)) continue;
                            
                            var kostType = _kostTypes.FirstOrDefault(kt => kt != null && kt.Type == kostTypeStr.Trim());
                            if (kostType != null)
                            {
                                // Rij voor het kosttype 
                                Grid rij = new Grid();
                                rij.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                                rij.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                                rij.Margin = new Thickness(0, 0, 0, 5);
                                
                                // Label
                                TextBlock txtLabel = new TextBlock
                                {
                                    Text = kostType.Text ?? "Onbekend kosttype",
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Margin = new Thickness(0, 0, 10, 0)
                                };
                                Grid.SetColumn(txtLabel, 0);
                                rij.Children.Add(txtLabel);
                                
                                // Invoerveld
                                string key = $"cost_{categorie.Nr}_{kostType.Type}";
                                TextBox txtInput = new TextBox
                                {
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Tag = key
                                };
                                
                                // Vul waarde in als beschikbaar
                                if (kostenWaarden.ContainsKey(key))
                                {
                                    txtInput.Text = kostenWaarden[key].ToString("F2");
                                }
                                
                                Grid.SetColumn(txtInput, 1);
                                rij.Children.Add(txtInput);
                                
                                // Voeg toe aan dictionary en panel
                                _invoerVelden[key] = txtInput;
                                panelContent.Children.Add(rij);
                                heeftContent = true;
                            }
                        }
                    }
                    
                    // Voeg vragen toe die bij deze categorie horen (zonder header)
                    var categorieVragen = _vragen?.Where(v => v != null && v.CategoryNr == categorie.Nr).ToList() ?? new List<Vraag>();
                    foreach (var vraag in categorieVragen)
                    {
                        if (vraag == null) continue;
                        
                        // Rij voor deze vraag
                        Grid rij = new Grid();
                        rij.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        rij.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                        rij.Margin = new Thickness(0, 0, 0, 5);
                        
                        // Label
                        TextBlock txtLabel = new TextBlock
                        {
                            Text = vraag.Text ?? "Onbekende vraag",
                            VerticalAlignment = VerticalAlignment.Center,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 0, 10, 0)
                        };
                        Grid.SetColumn(txtLabel, 0);
                        rij.Children.Add(txtLabel);
                        
                        // Invoerveld
                        string key = $"answer_{vraag.Id}";
                        TextBox txtInput = new TextBox
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            Tag = key
                        };
                        
                        // Vul waarde in als beschikbaar
                        if (antwoordWaarden.ContainsKey(key))
                        {
                            txtInput.Text = antwoordWaarden[key];
                        }
                        
                        Grid.SetColumn(txtInput, 1);
                        rij.Children.Add(txtInput);
                        
                        // Voeg toe aan dictionary en panel
                        _invoerVelden[key] = txtInput;
                        panelContent.Children.Add(rij);
                        heeftContent = true;
                    }
                    
                    // Voeg de content toe aan de expander
                    expCategorie.Content = panelContent;
                    
                    // Toon de expander alleen als er content is
                    if (heeftContent)
                    {
                        spnKostenCategorieën.Children.Add(expCategorie);
                    }
                }
                
                // Toon een bericht als er geen categorieën of velden zijn
                if (_invoerVelden.Count == 0)
                {
                    TextBlock txtGeenVelden = new TextBlock
                    {
                        Text = "Er zijn geen invoervelden beschikbaar. Mogelijk zijn er geen categorieën, kosttypes of vragen geconfigureerd in het systeem.",
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = Brushes.Orange,
                        Margin = new Thickness(0, 10, 0, 10)
                    };
                    spnKostenCategorieën.Children.Clear();
                    spnKostenCategorieën.Children.Add(txtGeenVelden);
                }
            }
            catch (Exception ex)
            {
                // Verbeterde foutmelding met meer details
                string bericht = $"Fout bij het laden van kostengegevens: {ex.Message}";
                if (ex.InnerException != null)
                {
                    bericht += $"\nInnerException: {ex.InnerException.Message}";
                }
                MessageBox.Show(bericht, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Log de fout voor debugging
                System.Diagnostics.Debug.WriteLine($"Exception in LaadKostenCategorieën: {ex}");
                
                // Voeg een foutmelding toe aan het panel
                spnKostenCategorieën.Children.Clear();
                TextBlock txtError = new TextBlock
                {
                    Text = "Er is een fout opgetreden bij het laden van de kosten en vragen. Probeer de pagina opnieuw te laden.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Red,
                    Margin = new Thickness(0, 10, 0, 10)
                };
                spnKostenCategorieën.Children.Add(txtError);
                
                Button btnVernieuw = new Button
                {
                    Content = "Pagina vernieuwen",
                    Style = (Style)FindResource("StandardButtonStyle"),
                    Margin = new Thickness(0, 10, 0, 10)
                };
                btnVernieuw.Click += (s, e) => VernieuwPagina();
                spnKostenCategorieën.Children.Add(btnVernieuw);
            }
        }

        private void SelectJaarrapport(object sender, RoutedEventArgs e)
        {
            // Deselecteer vorige button
            if (_geselecteerdRapportButton != null)
            {
                _geselecteerdRapportButton.Background = (Brush)FindResource("SecondaryColor");
            }

            // Markeer de nieuw geselecteerde button
            Button button = sender as Button;
            if (button != null)
            {
                button.Background = (Brush)FindResource("PrimaryColor");
                _geselecteerdRapportButton = button;
                
                // Haal het rapport ID uit de Tag property
                _geselecteerdRapportId = Convert.ToInt32(button.Tag);
                
                LaadRapportDetails(_geselecteerdRapportId.Value);
                btnVerwijderRapport.IsEnabled = true;
            }
        }

        private void LaadRapportDetails(int rapportId)
        {
            try
            {
                // Haal jaarrapportdetails op
                var details = _jaarrapportService.GetYearreportDetails(rapportId);
                
                if (details != null && details.ContainsKey("Jaarrapport"))
                {
                    var rapport = (Jaarrapport)details["Jaarrapport"];
                    
                    // Vul de basisgegevens in
                    txtJaar.Text = rapport.Year.ToString();
                    txtFTE.Text = rapport.Fte.ToString("F2");
                    
                    // Laad de kosten categorieën met waarden
                LaadKostenCategorieen();
                    
                    txtStatusBericht.Text = "Rapport geladen. Wijzig gegevens en klik op 'Rapport opslaan' om bij te werken.";
                }
                else
                {
                    txtStatusBericht.Text = "Kon rapportdetails niet laden.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het laden van rapportdetails: {ex.Message}",
                               "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNieuwRapport_Click(object sender, RoutedEventArgs e)
        {
            // Reset de velden voor een nieuw rapport
            _geselecteerdRapportId = null;
            
            // Deselecteer eventueel geselecteerde rapport
            if (_geselecteerdRapportButton != null)
            {
                _geselecteerdRapportButton.Background = (Brush)FindResource("SecondaryColor");
                _geselecteerdRapportButton = null;
            }
            
            // Schakel verwijderknop uit omdat dit een nieuw rapport is
            btnVerwijderRapport.IsEnabled = false;
            
            // Reset formulier
            txtJaar.Text = DateTime.Now.Year.ToString();
            txtFTE.Text = "";
            
            // Laad lege kostencategorieën
            LaadKostenCategorieen();
            
            txtStatusBericht.Text = "Vul de gegevens in voor het nieuwe jaarrapport.";
        }

        private void btnVerwijderRapport_Click(object sender, RoutedEventArgs e)
        {
            if (_geselecteerdRapportId.HasValue)
            {
                // Bevestiging vragen
                MessageBoxResult result = MessageBox.Show(
                    $"Weet u zeker dat u het jaarrapport voor {txtJaar.Text} wilt verwijderen?",
                    "Bevestiging verwijderen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Verwijder het rapport via de service
                        bool verwijderd = _jaarrapportService.DeleteYearreport(_geselecteerdRapportId.Value);
                        
                        if (verwijderd)
                        {
                        // UI bijwerken
                        if (_geselecteerdRapportButton != null)
                        {
                            spnJaarrapporten.Children.Remove(_geselecteerdRapportButton);
                        }
                        
                        // Reset selectie
                        _geselecteerdRapportButton = null;
                        _geselecteerdRapportId = null;
                        btnVerwijderRapport.IsEnabled = false;
                        
                        // Reset formulier
                        txtJaar.Text = "";
                        txtFTE.Text = "";
                        spnKostenCategorieën.Children.Clear();
                        
                        txtStatusBericht.Text = "Jaarrapport succesvol verwijderd.";
                        }
                        else
                        {
                            txtStatusBericht.Text = "Kon het jaarrapport niet verwijderen.";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij het verwijderen van het jaarrapport: {ex.Message}",
                                       "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnOpslaanRapport_Click(object sender, RoutedEventArgs e)
        {
            if (!ValideerFormulier())
            {
                return;
            }

            try
            {
                // Verzamel basisgegevens
                int jaar = int.Parse(txtJaar.Text.Trim());
                decimal fte = decimal.Parse(txtFTE.Text.Trim());
                
                // Maak een Jaarrapport object
                Jaarrapport rapport = new Jaarrapport
                {
                    Year = jaar,
                    Fte = fte,
                    CompanyId = App.IngelogdBedrijf.Id
                };
                
                // Voeg ID toe als we een bestaand rapport bijwerken
                if (_geselecteerdRapportId.HasValue)
                {
                    rapport.Id = _geselecteerdRapportId.Value;
                }
                
                // Verzamel kostengegevens
                List<Kost> kosten = new List<Kost>();
                List<Antwoord> antwoorden = new List<Antwoord>();
                
                foreach (var kvp in _invoerVelden)
                {
                    string key = kvp.Key;
                    TextBox textBox = kvp.Value;
                    string waarde = textBox.Text.Trim();
                    
                    if (!string.IsNullOrEmpty(waarde))
                    {
                        if (key.StartsWith("cost_"))
                        {
                            // Formaat is cost_categoryNr_costtypeType
                            var delen = key.Split('_');
                            if (delen.Length == 3 && decimal.TryParse(waarde, out decimal kostWaarde))
                            {
                                kosten.Add(new Kost
                                {
                                    CategoryNr = int.Parse(delen[1]),
                                    CosttypeType = delen[2],
                                    Value = kostWaarde
                                });
                            }
                        }
                        else if (key.StartsWith("answer_"))
                        {
                            // Formaat is answer_questionId
                            var delen = key.Split('_');
                            if (delen.Length == 2)
                            {
                                antwoorden.Add(new Antwoord
                                {
                                    QuestionId = int.Parse(delen[1]),
                                    Value = waarde
                                });
                            }
                        }
                    }
                }
                
                // Sla op of werk bij
                int rapportId;
                
                if (_geselecteerdRapportId.HasValue)
                {
                    // Bestaand rapport bijwerken
                    bool bijgewerkt = _jaarrapportService.UpdateYearreport(rapport);
                    if (!bijgewerkt)
                    {
                        throw new Exception("Kon het rapport niet bijwerken");
                    }
                    rapportId = rapport.Id;
                    txtStatusBericht.Text = $"Jaarrapport voor {jaar} is bijgewerkt.";
                }
                else
                {
                    // Nieuw rapport aanmaken
                    rapportId = _jaarrapportService.CreateYearreport(rapport);
                    if (rapportId <= 0)
                    {
                        throw new Exception("Kon het rapport niet aanmaken");
                    }
                    txtStatusBericht.Text = $"Nieuw jaarrapport voor {jaar} is aangemaakt.";
                }
                
                // Sla kosten en antwoorden op
                _jaarrapportService.SaveCostsForReport(rapportId, kosten);
                _jaarrapportService.SaveAnswersForReport(rapportId, antwoorden);
                
                // Herlaad de lijst met rapporten
                LaadJaarrapporten();
                
                // Selecteer het zojuist opgeslagen rapport
                foreach (UIElement element in spnJaarrapporten.Children)
                {
                    if (element is Button button && button.Tag != null && 
                        button.Tag.ToString() == rapportId.ToString())
                    {
                        SelectJaarrapport(button, null);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het opslaan van het jaarrapport: {ex.Message}",
                               "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValideerFormulier()
        {
            // Reset statusbericht
            txtStatusBericht.Text = "";
            
            // Controleer jaar
            if (string.IsNullOrWhiteSpace(txtJaar.Text))
            {
                txtStatusBericht.Text = "Vul een jaar in.";
                txtJaar.Focus();
                return false;
            }
            else if (!int.TryParse(txtJaar.Text, out int jaar) || jaar < 2000 || jaar > 2100)
            {
                txtStatusBericht.Text = "Vul een geldig jaar in (2000-2100).";
                txtJaar.Focus();
                return false;
            }
            
            // Controleer FTE
            if (string.IsNullOrWhiteSpace(txtFTE.Text))
            {
                txtStatusBericht.Text = "Vul het aantal FTE in.";
                txtFTE.Focus();
                return false;
            }
            else if (!decimal.TryParse(txtFTE.Text, out decimal fte) || fte <= 0)
            {
                txtStatusBericht.Text = "Vul een geldig aantal FTE in (groter dan 0).";
                txtFTE.Focus();
                return false;
            }
            
            // Valideer kostenwaarden
            foreach (var kvp in _invoerVelden)
            {
                string key = kvp.Key;
                TextBox textBox = kvp.Value;
                string waarde = textBox.Text.Trim();
                
                if (!string.IsNullOrEmpty(waarde) && key.StartsWith("cost_"))
                {
                    if (!decimal.TryParse(waarde, out decimal kostWaarde) || kostWaarde < 0)
                    {
                        txtStatusBericht.Text = "Vul geldige bedragen in (positieve getallen).";
                        textBox.Focus();
                        return false;
                    }
                }
            }
            
            return true;
        }

        private void btnTerugNaarDashboard_Click(object sender, RoutedEventArgs e)
        {
            // Navigeer terug naar dashboard
            NavigationService.Navigate(new CompanyDashboardPage());
        }
    }
} 