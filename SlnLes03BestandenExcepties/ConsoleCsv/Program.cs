using System;
using System.IO;

namespace WedstrijdenGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Arrays met mogelijke spelers, spellen en uitslagen
            string[] spelers = { "Zakaria", "Saleha", "Indra", "Ralph", "Francisco", "Marie" };
            string[] spellen = { "Schaken", "Dammen", "Backgammon" };
            string[] uitslagen = { "3-0", "2-1", "1-2", "0-3" };

            // Bepaal pad naar Desktop en stel bestandsnaam in
            string desktopPad = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string csvBestand = Path.Combine(desktopPad, "wedstrijden.csv");

            Random rnd = new Random();

            try
            {
                using (StreamWriter sw = new StreamWriter(csvBestand))
                {
                    for (int i = 1; i <= 100; i++)
                    {
                        // Kies twee verschillende spelers
                        int indexSpeler1 = rnd.Next(spelers.Length);
                        int indexSpeler2;
                        do
                        {
                            indexSpeler2 = rnd.Next(spelers.Length);
                        } while (indexSpeler2 == indexSpeler1);

                        string speler1 = spelers[indexSpeler1];
                        string speler2 = spelers[indexSpeler2];

                        // Kies een willekeurig spel en uitslag
                        string spel = spellen[rnd.Next(spellen.Length)];
                        string uitslag = uitslagen[rnd.Next(uitslagen.Length)];

                        // Voorbeeld: "1;Zakaria;Saleha;Schaken;2-1"
                        string regel = $"{i};{speler1};{speler2};{spel};{uitslag}";
                        sw.WriteLine(regel);
                    }
                }

                Console.WriteLine("Het bestand 'wedstrijden.csv' is succesvol aangemaakt op het bureaublad.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Er is een fout opgetreden bij het schrijven van het CSV-bestand:");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Druk op een toets om af te sluiten...");
            Console.ReadKey();
        }
    }
}
