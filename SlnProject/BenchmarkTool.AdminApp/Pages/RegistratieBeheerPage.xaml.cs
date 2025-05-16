using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BenchmarkTool.ClassLibrary.Data;
using BenchmarkTool.ClassLibrary.Models;

namespace BenchmarkTool.AdminApp.Pages
{
    /// <summary>
    /// Interaction logic for RegistratieBeheerPage.xaml
    /// </summary>
    public partial class RegistratieBeheerPage : Page
    {
        private readonly BedrijfRepository _bedrijfRepository;
        private List<Bedrijf> _registratieVerzoeken;
        private Bedrijf _geselecteerdVerzoek;

        public RegistratieBeheerPage()
        {
            InitializeComponent();
            _bedrijfRepository = new BedrijfRepository();
            
            LaadRegistratieVerzoeken();
        }

        private void LaadRegistratieVerzoeken()
        {
            try
            {
                // Haal alle registratieverzoeken op (status = "Pending")
                _registratieVerzoeken = _bedrijfRepository.GetPendingRegistrations();
                
                // Wis de huidige lijst (behalve de header)
                while (spRegistratieLijst.Children.Count > 1)
                {
                    spRegistratieLijst.Children.RemoveAt(1);
                }
                
                // Toon bericht als er geen verzoeken zijn
                if (_registratieVerzoeken.Count == 0)
                {
                    TextBlock tbGeenVerzoeken = new TextBlock
                    {
                        Text = "Er zijn momenteel geen openstaande registratieverzoeken.",
                        Margin = new Thickness(0, 20, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontStyle = FontStyles.Italic
                    };
                    
                    spRegistratieLijst.Children.Add(tbGeenVerzoeken);
                    return;
                }
                
                // Voeg elk verzoek toe aan de lijst
                foreach (var verzoek in _registratieVerzoeken)
                {
                    Border verzoekBorder = new Border();
                    verzoekBorder.Padding = new Thickness(10);
                    Grid verzoekGrid = new Grid();
                    verzoekBorder.Tag = verzoek;  // Koppel het bedrijfsobject aan de grid
                    verzoekBorder.Cursor = Cursors.Hand;
                    
                    // Definieer kolommen (zelfde als in de header)
                    for (int i = 0; i < 5; i++)
                    {
                        ColumnDefinition colDef = new ColumnDefinition();
                        if (i == 0) colDef.Width = new GridLength(50);
                        else if (i == 1) colDef.Width = new GridLength(200);
                        else if (i == 2 || i == 3) colDef.Width = new GridLength(150);
                        else colDef.Width = new GridLength(1, GridUnitType.Star);
                        
                        verzoekGrid.ColumnDefinitions.Add(colDef);
                    }
                    
                    // Voeg gegevens toe
                    TextBlock tbId = new TextBlock { Text = verzoek.Id.ToString() };
                    Grid.SetColumn(tbId, 0);
                    
                    TextBlock tbNaam = new TextBlock { Text = verzoek.Name };
                    Grid.SetColumn(tbNaam, 1);
                    
                    TextBlock tbContact = new TextBlock { Text = verzoek.Contact };
                    Grid.SetColumn(tbContact, 2);
                    
                    TextBlock tbDatum = new TextBlock { Text = verzoek.RegDate.ToString("dd/MM/yyyy") };
                    Grid.SetColumn(tbDatum, 3);
                    
                    // Voeg elementen toe aan grid
                    verzoekGrid.Children.Add(tbId);
                    verzoekGrid.Children.Add(tbNaam);
                    verzoekGrid.Children.Add(tbContact);
                    verzoekGrid.Children.Add(tbDatum);
                    
                    // Voeg event handlers toe
                    verzoekBorder.MouseLeftButtonDown += VerzoekBorder_MouseLeftButtonDown;
                    
                    // Voeg de grid toe aan de StackPanel
                    verzoekBorder.Child = verzoekGrid;
                    spRegistratieLijst.Children.Add(verzoekBorder);
                }
                
                // Reset geselecteerd verzoek
                _geselecteerdVerzoek = null;
                spGeselecteerdeDetails.Visibility = Visibility.Collapsed;
                btnGoedkeuren.IsEnabled = false;
                btnAfwijzen.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het laden van registratieverzoeken: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VerzoekBorder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Deselecteer het vorige geselecteerde item
            foreach (var child in spRegistratieLijst.Children)
            {
                if (child is Border border && border.Tag is Bedrijf)
                {
                    border.Background = Brushes.Transparent;
                }
            }
            
            // Selecteer het nieuwe item
            Border selectedBorder = sender as Border;
            if (selectedBorder != null)
            {
                selectedBorder.Background = Brushes.LightBlue;
                _geselecteerdVerzoek = selectedBorder.Tag as Bedrijf;
                
                // Toon details van het geselecteerde verzoek
                if (_geselecteerdVerzoek != null)
                {
                    txtGeselecteerdBedrijf.Text = _geselecteerdVerzoek.Name;
                    txtGeselecteerdContact.Text = $"Contact: {_geselecteerdVerzoek.Contact}";
                    txtGeselecteerdEmail.Text = $"E-mail: {_geselecteerdVerzoek.Email}";
                    
                    spGeselecteerdeDetails.Visibility = Visibility.Visible;
                    
                    // Activeer de knoppen
                    btnGoedkeuren.IsEnabled = true;
                    btnAfwijzen.IsEnabled = true;
                }
            }
        }

        private void btnTerug_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (Window.GetWindow(this) as AdminMainWindow);
            if (mainWindow != null)
            {
                mainWindow.NavigeerNaarDashboard();
            }
            else
            {
                // Fallback naar de oude methode
                NavigationService.Navigate(new AdminDashboardPage());
            }
        }

        private void btnGoedkeuren_Click(object sender, RoutedEventArgs e)
        {
            if (_geselecteerdVerzoek == null) return;
            
            // Toon een dialoogvenster om een initieel wachtwoord in te voeren
            PasswordInputDialog passwordDialog = new PasswordInputDialog();
            if (passwordDialog.ShowDialog() == true)
            {
                string wachtwoord = passwordDialog.Password;
                
                if (string.IsNullOrWhiteSpace(wachtwoord))
                {
                    MessageBox.Show("U moet een wachtwoord opgeven voor dit bedrijf.", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                try
                {
                    // Keur het verzoek goed met het opgegeven wachtwoord
                    bool success = _bedrijfRepository.ApproveRegistration(_geselecteerdVerzoek.Id, wachtwoord);
                    
                    if (success)
                    {
                        MessageBox.Show($"Het registratieverzoek van {_geselecteerdVerzoek.Name} is goedgekeurd.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Herlaad de lijst van registratieverzoeken
                        LaadRegistratieVerzoeken();
                    }
                    else
                    {
                        MessageBox.Show("Kon het registratieverzoek niet goedkeuren. Mogelijk bestaat het verzoek niet meer.", 
                            "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij het goedkeuren van het registratieverzoek: {ex.Message}", 
                        "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAfwijzen_Click(object sender, RoutedEventArgs e)
        {
            if (_geselecteerdVerzoek == null) return;
            
            // Vraag om bevestiging
            MessageBoxResult result = MessageBox.Show(
                $"Weet u zeker dat u het registratieverzoek van '{_geselecteerdVerzoek.Name}' wilt afwijzen?",
                "Bevestiging",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Wijs het verzoek af
                    bool success = _bedrijfRepository.RejectRegistration(_geselecteerdVerzoek.Id);
                    
                    if (success)
                    {
                        MessageBox.Show($"Het registratieverzoek van {_geselecteerdVerzoek.Name} is afgewezen.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Herlaad de lijst van registratieverzoeken
                        LaadRegistratieVerzoeken();
                    }
                    else
                    {
                        MessageBox.Show("Kon het registratieverzoek niet afwijzen. Mogelijk bestaat het verzoek niet meer.", 
                            "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij het afwijzen van het registratieverzoek: {ex.Message}", 
                        "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    /// <summary>
    /// Dialoogvenster om een wachtwoord in te voeren
    /// </summary>
    public class PasswordInputDialog : Window
    {
        private PasswordBox _passwordBox;
        
        public string Password { get; private set; }
        
        public PasswordInputDialog()
        {
            Title = "Wachtwoord invoeren";
            Width = 350;
            Height = 180;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            // Layout
            var grid = new Grid { Margin = new Thickness(15) };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            
            // Instructietekst
            var txtInstructions = new TextBlock 
            { 
                Text = "Voer een initieel wachtwoord in voor dit bedrijf:",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(txtInstructions, 0);
            
            // Wachtwoordveld
            _passwordBox = new PasswordBox 
            { 
                Margin = new Thickness(0, 0, 0, 15),
                Padding = new Thickness(5),
                PasswordChar = '*'
            };
            Grid.SetRow(_passwordBox, 1);
            
            // Knoppen
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            
            var btnOk = new Button
            {
                Content = "OK",
                IsDefault = true,
                Padding = new Thickness(10, 3, 10, 3),
                Margin = new Thickness(0, 0, 5, 0),
                MinWidth = 80
            };
            btnOk.Click += (s, e) => { Password = _passwordBox.Password; DialogResult = true; };
            
            var btnCancel = new Button
            {
                Content = "Annuleren",
                IsCancel = true,
                Padding = new Thickness(10, 3, 10, 3),
                MinWidth = 80
            };
            
            buttonPanel.Children.Add(btnOk);
            buttonPanel.Children.Add(btnCancel);
            Grid.SetRow(buttonPanel, 2);
            
            // Voeg controls toe aan grid
            grid.Children.Add(txtInstructions);
            grid.Children.Add(_passwordBox);
            grid.Children.Add(buttonPanel);
            
            Content = grid;
        }
    }
} 