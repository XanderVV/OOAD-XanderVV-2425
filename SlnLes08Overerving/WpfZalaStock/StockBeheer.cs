using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfZalaStock
{
    public class StockBeheer
    {
        public Dictionary<Product, int> Verkocht { get; private set; }
        public Dictionary<Product, int> Geretourneerd { get; private set; }

        public StockBeheer()
        {
            Verkocht = new Dictionary<Product, int>();
            Geretourneerd = new Dictionary<Product, int>();
        }

        public bool Verkopen(Product product, int aantal)
        {
            if (product == null || aantal <= 0)
                return false;

            if (product.Verkoop(aantal))
            {
                if (Verkocht.ContainsKey(product))
                {
                    Verkocht[product] += aantal;
                }
                else
                {
                    Verkocht.Add(product, aantal);
                }
                return true;
            }
            return false;
        }

        public bool Retourneren(Product product, int aantal)
        {
            if (product == null || aantal <= 0)
                return false;

            // Controleren of het product eerder is verkocht
            bool isVerkocht = Verkocht.ContainsKey(product) && Verkocht[product] >= aantal;
            
            if (isVerkocht)
            {
                if (product.Retourneer(aantal))
                {
                    if (Geretourneerd.ContainsKey(product))
                    {
                        Geretourneerd[product] += aantal;
                    }
                    else
                    {
                        Geretourneerd.Add(product, aantal);
                    }
                    return true;
                }
            }
            return false;
        }

        public decimal BerekenTotaalVerkocht()
        {
            decimal totaal = 0;
            foreach (var item in Verkocht)
            {
                totaal += item.Key.Prijs * item.Value;
            }
            return totaal;
        }

        public decimal BerekenTotaalGeretourneerd()
        {
            decimal totaal = 0;
            foreach (var item in Geretourneerd)
            {
                totaal += item.Key.Prijs * item.Value;
            }
            return totaal;
        }

        public decimal BerekenNetto()
        {
            return BerekenTotaalVerkocht() - BerekenTotaalGeretourneerd();
        }
    }
} 