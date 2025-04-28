using System;

namespace WpfZalaStock
{
    public class Kleding : Product
    {
        public Pasvorm Pasvorm { get; set; }
        public Lengte Lengte { get; set; }

        public Kleding(string naam, string merk, decimal prijs, string kleur, int aantalInStock, 
                      Pasvorm pasvorm, Lengte lengte) 
            : base(naam, merk, prijs, kleur, aantalInStock)
        {
            Pasvorm = pasvorm;
            Lengte = lengte;
        }
    }
} 