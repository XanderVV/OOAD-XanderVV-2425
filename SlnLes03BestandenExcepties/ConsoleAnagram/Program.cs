using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace ConsoleAnagram
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1) Lees het bestand in met meerdere catch-blokken:
            string[] alleWoorden;
            try
            {
                // Gebruik Path.Combine op basis van de huidige directory
                string bestandPad = Path.Combine(Environment.CurrentDirectory, "1000woorden.txt");
                alleWoorden = File.ReadAllLines(bestandPad);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Fout: het woordenbestand werd niet gevonden!");
                return; // stop het programma
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Fout bij lezen van het bestand: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Onverwachte fout: {ex.Message}");
                return;
            }

            // 2) Vraag aantal letters (tussen 5 en 15):
            Console.WriteLine("CONSOLE ANAGRAM");
            Console.WriteLine("================");
            int aantalLetters = 0;
            while (true)
            {
                Console.Write("Kies het aantal letters (5-15): ");
                string invoer = Console.ReadLine();
                if (int.TryParse(invoer, out int getal) && getal >= 5 && getal <= 15)
                {
                    aantalLetters = getal;
                    break;
                }
                Console.WriteLine("Ongeldige invoer. Probeer opnieuw.");
            }

            // 3) Filter met LINQ op lengte == aantalLetters
            var geschikteWoorden = alleWoorden
                .Where(w => w.Length == aantalLetters)
                .ToList();

            if (geschikteWoorden.Count == 0)
            {
                Console.WriteLine($"Er zijn geen woorden met {aantalLetters} letters.");
                return;
            }

            // 4) Kies één willekeurig woord
            Random rnd = new Random();
            string gekozenWoord = geschikteWoorden[rnd.Next(geschikteWoorden.Count)];

            // 5) Start een stopwatch om de tijd te meten
            Stopwatch stopwatch = Stopwatch.StartNew();

            // 6) Laat de gebruiker telkens Enter drukken voor een nieuw anagram
            //   en controleer zodra de gebruiker iets intypt (niet leeg).
            while (true)
            {
                // “In één regel” de letters doorheen schudden met LINQ:
                string anagram = new string(gekozenWoord.OrderBy(c => rnd.Next()).ToArray());

                Console.WriteLine();
                Console.WriteLine($"Anagram: {anagram}");
                Console.Write("Het woord (druk op enter om opnieuw te schudden): ");

                string input = Console.ReadLine();

                // 7) Als de gebruiker Enter drukt (lege input), nieuwe shuffle tonen.
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                // 8) Gebruiker heeft iets getypt => controleer goed of fout.
                stopwatch.Stop();
                if (input.Equals(gekozenWoord, StringComparison.OrdinalIgnoreCase))
                {
                    // Proficiat
                    Console.WriteLine(
                        $"Proficiat! Je hebt het woord geraden in {stopwatch.Elapsed.Minutes}m " +
                        $"{stopwatch.Elapsed.Seconds}s {stopwatch.Elapsed.Milliseconds}ms"
                    );
                }
                else
                {
                    // Helaas
                    Console.WriteLine($"Helaas! Het woord was '{gekozenWoord}'");
                }

                // Hier eindigt het programma
                break;
            }

            Console.WriteLine("\nDruk op een toets om af te sluiten...");
            Console.ReadKey();
        }
    }
}
