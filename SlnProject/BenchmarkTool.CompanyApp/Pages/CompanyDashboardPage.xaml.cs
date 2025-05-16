using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BenchmarkTool.CompanyApp.Pages
{
    /// <summary>
    /// Dashboard pagina voor bedrijfsgebruikers
    /// </summary>
    public partial class CompanyDashboardPage : Page
    {
        // Bewaar een referentie naar het hoofdvenster
        private CompanyMainWindow _hoofdVenster;

        public CompanyDashboardPage()
        {
            InitializeComponent();
            // Haal het hoofdvenster op
            _hoofdVenster = (CompanyMainWindow)Application.Current.MainWindow;
        }

        private void btnJaarrapporten_Click(object sender, MouseButtonEventArgs e)
        {
            _hoofdVenster.NavigeerNaar(new JaarrapportBeheerPage());
        }

        private void btnBenchmark_Click(object sender, MouseButtonEventArgs e)
        {
            _hoofdVenster.NavigeerNaar(new BenchmarkPage());
        }

        private void btnUitloggen_Click(object sender, RoutedEventArgs e)
        {
            // Bevestiging dialoog tonen
            MessageBoxResult result = MessageBox.Show("Weet u zeker dat u wilt uitloggen?",
                                                     "Uitloggen",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Navigeer terug naar de loginpagina
                _hoofdVenster.NavigeerNaar(new CompanyLoginPage());
            }
        }
    }
} 