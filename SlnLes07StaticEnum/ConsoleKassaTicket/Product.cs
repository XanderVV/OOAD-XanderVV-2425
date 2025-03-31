using System;

namespace ConsoleKassaTicket
{
    public class Product
    {
        // Properties
        public string Naam { get; set; }
        public decimal Eenheidsprijs { get; set; }
        public string Code { get; private set; }

        // Lege constructor
        public Product()
        {
        }

        // Overloaded constructor
        public Product(string naam, decimal eenheidsprijs, string code)
        {
            Naam = naam;
            Eenheidsprijs = eenheidsprijs;
            Code = VerwerkCode(code); // Zet de code direct om en valideer
        }

        // Statische methode voor code‐validatie
        // Eis: 6 tekens, begint met 'P' (hoofdletter of kleine letter)
        public static bool ValideerCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            // Maak hoofdletter van het eerste teken als men 'p' gebruikt
            code = code.ToUpper();

            // Check lengte en eerste karakter
            if (code.Length != 6) return false;
            if (!code.StartsWith("P")) return false;

            // Check dat de resterende 5 tekens cijfers zijn
            for (int i = 1; i < 6; i++)
            {
                if (!char.IsDigit(code[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // Hulpmethode om de code correct op te slaan (convert + validatie)
        private string VerwerkCode(string code)
        {
            // Bv. als men "p01234" meegeeft, dan omzetten naar "P01234"
            code = code.ToUpper();
            if (!ValideerCode(code))
            {
                throw new ArgumentException($"Ongeldige productcode: {code}");
            }
            return code;
        }

        // ToString - formaat volgens opdracht: “(P45612) bananen 1.24”
        public override string ToString()
        {
            // Let op regionale decimale komma/punt; indien komma gewenst:
            // gebruik string‐interpolatie met bijvoorbeeld :0.00 
            return $"({Code}) {Naam}: {Eenheidsprijs:0.00}";
        }
    }
}
