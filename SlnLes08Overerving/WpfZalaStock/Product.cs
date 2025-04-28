using System;

namespace WpfZalaStock
{
    public class Product
    {
        public string Naam { get; set; }
        public string Merk { get; set; }
        public decimal Prijs { get; set; }
        public string Kleur { get; set; }
        public int AantalInStock { get; set; }

        public Product(string naam, string merk, decimal prijs, string kleur, int aantalInStock)
        {
            Naam = naam;
            Merk = merk;
            Prijs = prijs;
            Kleur = kleur;
            AantalInStock = aantalInStock;
        }

        public virtual bool Verkoop(int aantal)
        {
            if (aantal <= 0)
                return false;

            if (AantalInStock >= aantal)
            {
                AantalInStock -= aantal;
                return true;
            }
            
            return false;
        }

        public virtual bool Retourneer(int aantal)
        {
            if (aantal <= 0)
                return false;

            AantalInStock += aantal;
            return true;
        }

        public override string ToString()
        {
            return $"{Naam} ({Merk}) - €{Prijs.ToString("0.00").Replace(".", ",")}";
        }
    }
} 