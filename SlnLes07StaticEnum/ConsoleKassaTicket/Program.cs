using System;

namespace ConsoleKassaTicket
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Voorbeeldproducten
            Product p1 = new Product("bananen", 1.75m, "P02384");
            // Test op kleine 'p' in de code (wordt intern omgezet naar hoofdletter):
            Product p2 = new Product("brood", 2.10m, "p01820");
            Product p3 = new Product("kaas", 3.99m, "P45612");
            Product p4 = new Product("koffie", 4.10m, "P98754");

            // Ticket aanmaken met kassier "Annie", betaald met Visa
            Ticket ticket = new Ticket("Annie", Betaalwijze.Visa);

            // Producten toevoegen
            ticket.Producten.Add(p1);
            ticket.Producten.Add(p2);
            ticket.Producten.Add(p3);
            ticket.Producten.Add(p4);

            // Afdrukken
            Console.WriteLine(ticket.PrintOut());

            // Wachten tot gebruiker een toets indrukt (optioneel)
            Console.ReadKey();
        }
    }
}
