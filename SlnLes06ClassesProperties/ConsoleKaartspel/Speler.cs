using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleKaartspel1
{
    internal class Speler
    {
        public string Naam { get;  set; }
        public List<Kaart> Kaarten { get; set; } = new List<Kaart> { };
        public bool HeeftNogKaarten
        {
            get
            {
                return Kaarten.Count > 0; // Controleer of de speler nog kaarten heeft
            }
        } 
        public Speler()
        {
        }

        // Methode voor de speler om een willekeurige kaart te leggen
        public Kaart LegKaart()
        {
            if (Kaarten.Count == 0) return null;
            Random rng = new Random(); // Random generator
            int nummer = rng.Next(Kaarten.Count); // Kies een willekeurige kaart
            Kaart kaart = Kaarten[nummer];
            Kaarten.RemoveAt(nummer); // Verwijder de kaart uit de handen van de speler
            return kaart;
        }
    }
}
