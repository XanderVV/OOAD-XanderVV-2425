using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;

namespace WpfMatchFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void KiesBestand1_Click(object sender, RoutedEventArgs e)
        {
            string bestandPad = OpenFile();
            if (bestandPad != null)
            {
                BestandPad1.Text = bestandPad;
            }
        }

        private void KiesBestand2_Click(object sender, RoutedEventArgs e)
        {
            string bestandPad = OpenFile();
            if (bestandPad != null)
            {
                BestandPad2.Text = bestandPad;
            }
        }

        private string OpenFile()
        {
            var openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "Tekstbestanden (*.txt)|*.txt|Alle bestanden (*.*)|*.*"
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        private void VergelijkBestanden_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BestandPad1.Text) || string.IsNullOrWhiteSpace(BestandPad2.Text))
            {
                MessageBox.Show(
                    "Selecteer alstublieft twee bestanden.",
                    "Fout",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            try
            {
                List<string> triplets1 = LeesTriplets(BestandPad1.Text);
                List<string> triplets2 = LeesTriplets(BestandPad2.Text);

                double percentage = BerekenOvereenkomst(triplets1, triplets2);
                ResultaatTextBlock.Text = $"De bestanden komen overeen voor {Math.Floor(percentage)}%";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Er is een fout opgetreden: {ex.Message}",
                    "Fout",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private List<string> LeesTriplets(string path)
        {
            string inhoud = File.ReadAllText(path);

            // Vervang alle niet-alfabetische tekens door spaties
            string schoneTekst = Regex.Replace(inhoud, "[^a-zA-Z]+", " ");

            // Verwijder meerdere spaties
            schoneTekst = Regex.Replace(schoneTekst, @"\s+", " ").Trim();

            // Split de tekst in woorden
            string[] woorden = schoneTekst.Split(' ');

            // Genereer unieke triplets
            var triplets = new HashSet<string>();

            for (int i = 0; i < woorden.Length - 2; i++)
            {
                string triplet = $"{woorden[i]} {woorden[i + 1]} {woorden[i + 2]}";
                triplets.Add(triplet);
            }

            return triplets.ToList();
        }

        private double BerekenOvereenkomst(List<string> lijst1, List<string> lijst2)
        {
            if (lijst1.Count == 0 || lijst2.Count == 0)
            {
                return 0.0;
            }

            // Tel het aantal gemeenschappelijke triplets
            int gemeenschappelijk = lijst1.Intersect(lijst2).Count();

            // Bereken het percentage van overeenkomst
            double percentage = (2.0 * gemeenschappelijk) / (lijst1.Count + lijst2.Count) * 100;
            return percentage;
        }
    }
}
