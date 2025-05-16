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
    /// Interaction logic for BedrijvenBeheerPage.xaml
    /// </summary>
    public partial class BedrijvenBeheerPage : Page
    {
        private readonly BedrijfRepository _bedrijfRepository;
        private List<Bedrijf> _bedrijven;
        private Bedrijf _geselecteerdBedrijf;

        public BedrijvenBeheerPage()
        {
            InitializeComponent();
            _bedrijfRepository = new BedrijfRepository();
            
            LaadBedrijven();
        }

        private void LaadBedrijven()
        {
            try
            {
                // Haal alle bedrijven op
                _bedrijven = _bedrijfRepository.GetAll();
                
                // Wis de huidige lijst (behalve de header)
                while (spBedrijvenLijst.Children.Count > 1)
                {
                    spBedrijvenLijst.Children.RemoveAt(1);
                }
                
                // Voeg elk bedrijf toe aan de lijst
                foreach (var bedrijf in _bedrijven)
                {
                    // Maak een border en grid
                    Border bedrijfBorder = new Border();
                    bedrijfBorder.Padding = new Thickness(10);
                    bedrijfBorder.Tag = bedrijf;  // Koppel het bedrijfsobject aan de border
                    bedrijfBorder.Cursor = Cursors.Hand;
                    
                    Grid bedrijfGrid = new Grid();
                    
                    // Definieer kolommen (zelfde als in de header)
                    for (int i = 0; i < 5; i++)
                    {
                        ColumnDefinition colDef = new ColumnDefinition();
                        if (i == 0) colDef.Width = new GridLength(50);
                        else if (i == 1) colDef.Width = new GridLength(200);
                        else if (i == 2 || i == 3) colDef.Width = new GridLength(150);
                        else colDef.Width = new GridLength(1, GridUnitType.Star);
                        
                        bedrijfGrid.ColumnDefinitions.Add(colDef);
                    }
                    
                    // Voeg gegevens toe
                    TextBlock tbId = new TextBlock { Text = bedrijf.Id.ToString() };
                    Grid.SetColumn(tbId, 0);
                    
                    TextBlock tbNaam = new TextBlock { Text = bedrijf.Name };
                    Grid.SetColumn(tbNaam, 1);
                    
                    TextBlock tbStatus = new TextBlock { Text = bedrijf.Status };
                    Grid.SetColumn(tbStatus, 2);
                    
                    TextBlock tbNacecode = new TextBlock { Text = bedrijf.NacecodeCode };
                    Grid.SetColumn(tbNacecode, 3);
                    
                    // Voeg elementen toe aan grid
                    bedrijfGrid.Children.Add(tbId);
                    bedrijfGrid.Children.Add(tbNaam);
                    bedrijfGrid.Children.Add(tbStatus);
                    bedrijfGrid.Children.Add(tbNacecode);
                    
                    // Voeg grid toe aan border
                    bedrijfBorder.Child = bedrijfGrid;
                    
                    // Voeg event handlers toe
                    bedrijfBorder.MouseLeftButtonDown += BedrijfGrid_MouseLeftButtonDown;
                    
                    // Voeg de border toe aan de StackPanel
                    spBedrijvenLijst.Children.Add(bedrijfBorder);
                }
                
                // Zet knoppen op inactief tot er een bedrijf wordt geselecteerd
                btnWijzigen.IsEnabled = false;
                btnVerwijderen.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het laden van bedrijven: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BedrijfGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Deselecteer het vorige geselecteerde item
            foreach (var child in spBedrijvenLijst.Children)
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
                _geselecteerdBedrijf = selectedBorder.Tag as Bedrijf;
                
                // Activeer de knoppen voor wijzigen en verwijderen
                btnWijzigen.IsEnabled = true;
                btnVerwijderen.IsEnabled = true;
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

        private void btnToevoegen_Click(object sender, RoutedEventArgs e)
        {
            // Navigeer naar het bedrijf toevoegen formulier
            NavigationService.Navigate(new BedrijfFormulierPage());
        }

        private void btnWijzigen_Click(object sender, RoutedEventArgs e)
        {
            if (_geselecteerdBedrijf != null)
            {
                try
                {
                    // Navigeer naar het bedrijf bewerken formulier met het geselecteerde bedrijf ID
                    NavigationService.Navigate(new BedrijfFormulierPage(_geselecteerdBedrijf.Id));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij het openen van het wijzigingsformulier: {ex.Message}", 
                        "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnVerwijderen_Click(object sender, RoutedEventArgs e)
        {
            if (_geselecteerdBedrijf != null)
            {
                // Vraag om bevestiging
                MessageBoxResult result = MessageBox.Show(
                    $"Weet u zeker dat u het bedrijf '{_geselecteerdBedrijf.Name}' wilt verwijderen?",
                    "Bevestiging",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Verwijder het bedrijf
                        bool success = _bedrijfRepository.Delete(_geselecteerdBedrijf.Id);
                        
                        if (success)
                        {
                            MessageBox.Show("Bedrijf is succesvol verwijderd.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                            
                            // Herlaad de lijst van bedrijven
                            LaadBedrijven();
                        }
                        else
                        {
                            MessageBox.Show("Kon het bedrijf niet verwijderen. Mogelijk bestaat het bedrijf niet meer.", 
                                "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij het verwijderen van het bedrijf: {ex.Message}", 
                            "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
} 