using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfZalaStock
{
    public class Sieraad : Product
    {
        public List<Materiaal> Materialen { get; set; }

        public Sieraad(string naam, string merk, decimal prijs, string kleur, int aantalInStock,
                      List<Materiaal> materialen) 
            : base(naam, merk, prijs, kleur, aantalInStock)
        {
            Materialen = materialen ?? new List<Materiaal>();
        }
    }
} 