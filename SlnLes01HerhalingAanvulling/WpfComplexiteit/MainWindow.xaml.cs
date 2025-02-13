using System.Linq;
using System.Windows;

namespace WpfComplexiteit
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string woord = txtwoord.Text;
            int aantalKarakters = woord.Length;
            int aantalLettergrepen = AantalLettergrepen(woord);
            double complexiteit = BerekenComplexiteit(woord);

            resultaatTextBlock.Text = $@"aantal karakters: {aantalKarakters}
aantal lettergrepen: {aantalLettergrepen}
complexiteit: {complexiteit:F1}";
        }

        private bool IsKlinker(char c)
        {
            return "aeiouAEIOU".Contains(char.ToUpper(c));
        }

        private int AantalLettergrepen(string woord)
        {
            int count = 0;
            for (int i = 0; i < woord.Length; i++)
            {
                if (IsKlinker(woord[i]) && (i == 0 || !IsKlinker(woord[i - 1])))
                {
                    count++;
                }
            }
            return count;
        }

        private double BerekenComplexiteit(string woord)
        {
            double c = (double)woord.Length / 3 + AantalLettergrepen(woord);

            if (woord.Contains('x')) c += 1;
            if (woord.Contains('y')) c += 1;
            if (woord.Contains('q')) c += 1;

            return c;
        }
    }
}
