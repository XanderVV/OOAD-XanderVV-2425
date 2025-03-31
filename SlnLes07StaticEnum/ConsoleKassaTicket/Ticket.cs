using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleKassaTicket
{
    public class Ticket
    {
        // Properties
        public List<Product> Producten { get; } = new List<Product>();
        public Betaalwijze BetaaldMet { get; set; } = Betaalwijze.Cash;  // standaard Cash
        public string Kassier { get; set; }

        // Totaalprijs: som van alle product‐eenheidsprijzen (read‐only)
        public decimal Totaalprijs
        {
            get
            {
                return Producten.Sum(p => p.Eenheidsprijs);
            }
        }

        // Constructor waarbij kassier en betaalwijze verplicht zijn
        public Ticket(string kassier, Betaalwijze betaalwijze)
        {
            Kassier = kassier;
            BetaaldMet = betaalwijze;
        }

        // Methode om het kassabonnement af te drukken
        public string PrintOut()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("KASSATICKET");
            sb.AppendLine("=============");
            sb.AppendLine($"Uw kassier: {Kassier}");

            // Elk product afdrukken
            foreach (var product in Producten)
            {
                sb.AppendLine(product.ToString());
            }

            sb.AppendLine("-----------");

            decimal totaal = Totaalprijs;

            // Als er betaald wordt met Visa, komt er €0,12 bij
            if (BetaaldMet == Betaalwijze.Visa)
            {
                sb.AppendLine("Visa kosten: 0,12");
                totaal += 0.12m;
            }

            sb.AppendLine($"Totaal: {totaal:0.00}");
            return sb.ToString();
        }
    }
}
