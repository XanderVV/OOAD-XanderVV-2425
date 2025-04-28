using System;

namespace WpfZalaStock
{
    public class Schoen : Product
    {
        public Breedte Breedte { get; set; }
        public Sluiting Sluiting { get; set; }
        public Neus Neus { get; set; }

        public Schoen(string naam, string merk, decimal prijs, string kleur, int aantalInStock,
                     Breedte breedte, Sluiting sluiting, Neus neus) 
            : base(naam, merk, prijs, kleur, aantalInStock)
        {
            Breedte = breedte;
            Sluiting = sluiting;
            Neus = neus;
        }
    }
} 