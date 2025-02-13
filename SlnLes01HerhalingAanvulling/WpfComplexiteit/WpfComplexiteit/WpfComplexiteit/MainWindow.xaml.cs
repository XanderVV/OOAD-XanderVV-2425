using System;
using System.Globalization;
using System.Windows;

namespace WpfComplexiteit
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnAnalyseer_Click(object sender, RoutedEventArgs e)
        {
            string invoer = txtWoord.Text;

            if (string.IsNullOrEmpty(invoer))
            {
                tblOutput.Text = string.Empty;
                return;
            }

            int aantalKarakters = invoer.Length;
            int lettergrepen = AantalLettergrepen(invoer);
            double comp = Complexiteit(invoer);

            string compString = comp.ToString("0.0", CultureInfo.InvariantCulture)
                                  .Replace('.', ',');

            tblOutput.Text = $@"aantal karakters: {aantalKarakters}
aantal lettergrepen: {lettergrepen}
complexiteit: {compString}";
        }



        private bool IsKlinker(char c)
        {
            char lower = char.ToLower(c);
            return "aeiou".IndexOf(lower) >= 0;
        }

        private int AantalLettergrepen(string woord)
        {
            int count = 0;
            bool vorigeWasKlinker = false;

            foreach (char c in woord)
            {
                if (IsKlinker(c))
                {
                    if (!vorigeWasKlinker)
                    {
                        count++;
                    }
                    vorigeWasKlinker = true;
                }
                else
                {
                    vorigeWasKlinker = false;
                }
            }

            return count;
        }

        private double Complexiteit(string woord)
        {
            int aantalLetters = woord.Length;
            int lettergrepen = AantalLettergrepen(woord);

            int extra = 0;
            foreach (char c in woord.ToLower())
            {
                if (c == 'x' || c == 'y' || c == 'q')
                {
                    extra++;
                }
            }

            double raw = (aantalLetters / 3.0) + lettergrepen + extra;
            return Math.Round(raw, 1);
        }
    }
}
