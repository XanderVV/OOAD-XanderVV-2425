using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleKaartspel1
{
    internal class Kaart
    {
        private int _nummer;
        private char _kleur;
        public int Nummer
        {
            get
            {
                return _nummer;
            }
            set
            {
                {
                    // Valideer het nummer, dit moet tussen 1 en 13 liggen
                    if (value < 1 || value > 13)
                    {
                        throw new ArgumentException("Nummer moet tussen 1 en 13 zijn.");
                    }
                    _nummer = value;
                }
            }
        }

        public char Kleur
        {
            get
            {
                return _kleur;
            }
            set
            {
                // Valideer de kleur,
                if (value != 'C' && value != 'S' && value != 'H' && value != 'D') 
                {
                    throw new ArgumentException("Ongeldige kleur.");
                }
                _kleur = value;
            }
        }

        // Constructor om een kaart te creëren
        public Kaart(int nummer, char kleur)
        {
            Nummer = nummer;
            Kleur = kleur;
        }
    }
}
