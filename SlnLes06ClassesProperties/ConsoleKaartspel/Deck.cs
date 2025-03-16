using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleKaartspel1
{
    internal class Deck
    {
        int dertien = 13;
        public List<Kaart> Kaarten { get; set; } // Lijst om de kaarten in het deck op te slaan

        // Constructor om een nieuw deck van 52 kaarten te creëren
        public Deck()
        {
            Kaarten = new List<Kaart>();
            char[] kleuren = { 'C', 'S', 'H', 'D' }; // De mogelijke kleuren van de kaarten in een array
            foreach (char kleur in kleuren)
            {
                for (int nummer = 1; nummer <= dertien; nummer++) // Voor elk nummer van 1 tot en met 13
                {
                    Kaarten.Add(new Kaart(nummer, kleur)); // Voeg een kaart toe aan het deck
                }
            }
        }

        // Methode om het deck te schudden
        public void Schudden()
        {
            Random rng = new Random(); // Random generator
            int n = Kaarten.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1); // Kies een willekeurige kaart
                Kaart waarde = Kaarten[k];
                Kaarten[k] = Kaarten[n];
                Kaarten[n] = waarde; // Wissel de kaarten van plaats
            }
        }

        // Methode om een kaart van het deck te nemen
        public Kaart NeemKaart()
        {
            if (Kaarten.Count > 0)
            {
                Kaart kaart = Kaarten[0]; // Neem de bovenste kaart
                Kaarten.RemoveAt(0); // Verwijder deze kaart uit het deck
                return kaart;
            }
            else
            {
                throw new InvalidOperationException("Geen kaarten meer in het deck.");
            }
        }
    }
}
