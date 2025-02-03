using System;
using System.Globalization;

namespace ConsoleComplexiteit
{
    internal class Program
    {
        private static void Main()
        {
            while (true)
            {
                Console.Write("Geef een woord (Enter om te stoppen): ");
                string invoer = Console.ReadLine();

                // Stoppen als de invoer leeg is
                if (string.IsNullOrEmpty(invoer))
                {
                    break;
                }

                int aantalKarakters = invoer.Length;
                int lettergrepen = AantalLettergrepen(invoer);
                double comp = Complexiteit(invoer);

                Console.WriteLine($"aantal karakters: {aantalKarakters}");
                Console.WriteLine($"aantal lettergrepen: {lettergrepen}");

                // Eén decimaal, met komma i.p.v. punt
                string compString = comp.ToString("0.0", CultureInfo.InvariantCulture)
                                        .Replace('.', ',');
                Console.WriteLine($"complexiteit: {compString}");
                Console.WriteLine(); // Lege regel na de resultaten
            }

            // Extra lege regel vóór "Bedankt en tot ziens."
            Console.WriteLine();
            Console.WriteLine("Bedankt en tot ziens.");
        }

        private static bool IsKlinker(char c)
        {
            char lower = char.ToLower(c);
            return "aeiou".IndexOf(lower) >= 0;
        }

        private static int AantalLettergrepen(string woord)
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

        private static double Complexiteit(string woord)
        {
            int aantalLetters = woord.Length;
            int lettergrepen = AantalLettergrepen(woord);

            // Tel hoe vaak x, y of q voorkomt
            int extra = 0;
            foreach (char c in woord.ToLower())
            {
                if (c == 'x' || c == 'y' || c == 'q')
                {
                    extra++;
                }
            }

            // Formule
            double raw = (aantalLetters / 3.0) + lettergrepen + extra;

            // Afronden op 1 decimaal
            return Math.Round(raw, 1);
        }
    }
}
