using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace WpfZalaStock
{
    public static class ProductParser
    {
        public static List<Product> LeesProducten(string bestandspad)
        {
            List<Product> producten = new List<Product>();

            try
            {
                if (!File.Exists(bestandspad))
                {
                    MessageBox.Show($"Bestand niet gevonden: {bestandspad}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return producten;
                }

                string[] regels = File.ReadAllLines(bestandspad);

                foreach (string regel in regels)
                {
                    try
                    {
                        string[] velden = regel.Split(';');
                        if (velden.Length < 6) // Minimaal categorie, naam, merk, prijs, kleur, aantal
                        {
                            MessageBox.Show($"Ongeldige regel: {regel}", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        string categorie = velden[0];
                        string naam = velden[1];
                        string merk = velden[2];
                        
                        // Vervang komma's door punten voor parsing
                        string prijsStr = velden[3].Replace(',', '.');
                        decimal prijs = decimal.Parse(prijsStr, CultureInfo.InvariantCulture);
                        
                        string kleur = velden[4];
                        int aantal = int.Parse(velden[5]);

                        Product product = null;

                        switch (categorie.ToLower())
                        {
                            case "kleding":
                                if (velden.Length >= 8)
                                {
                                    Pasvorm pasvorm = Enum.Parse<Pasvorm>(velden[6], true);
                                    Lengte lengte = Enum.Parse<Lengte>(velden[7], true);
                                    product = new Kleding(naam, merk, prijs, kleur, aantal, pasvorm, lengte);
                                }
                                break;
                            case "schoenen":
                                if (velden.Length >= 9)
                                {
                                    Breedte breedte = Enum.Parse<Breedte>(velden[6], true);
                                    Sluiting sluiting = Enum.Parse<Sluiting>(velden[7], true);
                                    Neus neus = Enum.Parse<Neus>(velden[8], true);
                                    product = new Schoen(naam, merk, prijs, kleur, aantal, breedte, sluiting, neus);
                                }
                                break;
                            case "sieraden":
                                List<Materiaal> materialen = new List<Materiaal>();
                                for (int i = 6; i < velden.Length; i++)
                                {
                                    if (Enum.TryParse<Materiaal>(velden[i], true, out Materiaal materiaal))
                                    {
                                        materialen.Add(materiaal);
                                    }
                                }
                                product = new Sieraad(naam, merk, prijs, kleur, aantal, materialen);
                                break;
                            default:
                                MessageBox.Show($"Onbekende categorie: {categorie}", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
                                break;
                        }

                        if (product != null)
                        {
                            producten.Add(product);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij verwerken regel: {regel}\n{ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij inlezen bestand: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return producten;
        }
    }
} 