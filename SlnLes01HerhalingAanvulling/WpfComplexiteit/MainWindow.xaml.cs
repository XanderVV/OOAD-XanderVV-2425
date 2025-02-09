using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfComplexiteit
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string woord = txtwoord.Text;
            {
                int karakter = woord.Length;
                int aantalLettergrepen = AantalLettergrepen(woord);
                double complexiteit = BerekenComplexiteit(woord);

                resultaatTextBlock.Text = $@"Aantal klinkers: {karakter}
Aantal lettergrepen: {aantalLettergrepen}
Complexiteit: {complexiteit:F1}";
            }
        }

        private bool IsKlinker(char c)
        {
            return "aeiouAEIOU".Contains(char.ToUpper(c));
        }

        private int AantalLettergrepen(string woord)
        {
            int aantalLettergrepen = 0;

            for (int i = 0; i < woord.Length; i++)
            {
                if (IsKlinker(woord[i]) && (i == 0 || !IsKlinker(woord[i - 1])))
                {
                    aantalLettergrepen++;
                }
            }
            return aantalLettergrepen;
        }

        private double BerekenComplexiteit(string woord)
        {
            double complexiteit = (double)woord.Length / 3 + AantalLettergrepen(woord);

            if (woord.Contains('x'))
            {
                complexiteit += 1;
            }

            if (woord.Contains('y'))
            {
                complexiteit += 1;
            }

            if (woord.Contains('q'))
            {
                complexiteit += 1;
            }
            return complexiteit;
        }
    }
}