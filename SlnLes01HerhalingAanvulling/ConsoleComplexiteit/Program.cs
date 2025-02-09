using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleComplexiteit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string woord;
            do
            {
                Console.Write("Geef een woord (enter om te stoppen): ");
                woord = Console.ReadLine();
                if (!string.IsNullOrEmpty(woord))
                {
                    int karakter = woord.Length;
                    int aantalLettergrepen = AantalLettergrepen(woord);
                    double complexiteit = BerekenComplexiteit(woord);
                    Console.WriteLine($"Aantal karakters in: {karakter}\n Aantal lettergrepen: {aantalLettergrepen}\n Complexiteit van : {complexiteit:F1}\n");
                }
                else
                {
                    Console.WriteLine();
                }
            }
            while (!string.IsNullOrEmpty(woord));
            Console.WriteLine("Bedankt en tot ziens.");
            Console.ReadKey();
        }

        static bool IsKlinker(char c)
        {
            return "aeiouAEIOU".Contains(char.ToUpper(c));
        }

        static int AantalLettergrepen(string woord)
        {
            int aantalLettergrepen = 0;

            for (int i = 0; i < woord.Length; i++)
            {
                if (IsKlinker(woord[i]) && (i == 0 || !IsKlinker(woord[i - 1])))
                {
                    aantalLettergrepen++;
                }
            }
            return aantalLettergrepen;
        }

        static double BerekenComplexiteit(string woord)
        {
            double complexiteit = (double)woord.Length / 3 + AantalLettergrepen(woord);

            if (woord.Contains('x'))
            {
                complexiteit += 1;
            }

            if (woord.Contains('y'))
            {
                complexiteit += 1;
            }

            if (woord.Contains('q'))
            {
                complexiteit += 1;
            } 
            return complexiteit;
        }
    }
}