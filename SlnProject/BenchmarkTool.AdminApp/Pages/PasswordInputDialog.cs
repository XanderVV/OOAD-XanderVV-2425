using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BenchmarkTool.AdminApp.Pages
{
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
            btnOk.Click += (s, e) => 
            { 
                Password = _passwordBox.Password; 
                DialogResult = true; 
            };
            
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