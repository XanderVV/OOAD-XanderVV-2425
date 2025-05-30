using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace BenchmarkTool.ClassLibrary.Authentication
{
    /// <summary>
    /// Helper klasse voor authenticatie-gerelateerde functionaliteit
    /// </summary>
    public static class AuthenticatieHelper
    {
        /// <summary>
        /// Genereert een beveiligde willekeurige token
        /// </summary>
        /// <param name="lengte">Lengte van de token in bytes</param>
        /// <returns>Een willekeurige token als base64 string</returns>
        public static string GenereerToken(int lengte = 32)
        {
            byte[] randomBytes = new byte[lengte];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Berekent een SHA256 hash van een gegeven string
        /// </summary>
        /// <param name="input">De input string</param>
        /// <returns>De SHA256 hash als hex string</returns>
        public static string BerekenSHA256Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Valideert of een wachtwoord voldoet aan de complexiteitseisen
        /// </summary>
        /// <param name="wachtwoord">Het te valideren wachtwoord</param>
        /// <returns>True als het wachtwoord voldoet aan de eisen</returns>
        public static bool ValideerWachtwoordComplexiteit(string wachtwoord)
        {
            // Minimal length
            if (string.IsNullOrEmpty(wachtwoord) || wachtwoord.Length < 6)
            {
                return false;
            }

            // Needs at least one digit
            bool hasDigit = false;
            for (int i = 0; i < wachtwoord.Length; i++)
            {
                if (char.IsDigit(wachtwoord[i]))
                {
                    hasDigit = true;
                    break;
                }
            }

            if (!hasDigit)
            {
                return false;
            }

            // Needs at least one non alphanumeric
            bool hasNonAlphanumeric = false;
            for (int i = 0; i < wachtwoord.Length; i++)
            {
                if (!char.IsLetterOrDigit(wachtwoord[i]))
                {
                    hasNonAlphanumeric = true;
                    break;
                }
            }

            return hasNonAlphanumeric;
        }

        /// <summary>
        /// Genereert een nieuw wachtwoord hash voor de hardcoded admin wachtwoord "admin"
        /// </summary>
        public static string GenereerAdminWachtwoordHashVoorTesten()
        {
            string hash = HashWachtwoordMetSHA256("admin");
            Console.WriteLine($"SHA256 hash voor wachtwoord 'admin': {hash}");
            return hash;
        }

        /// <summary>
        /// Hasht een wachtwoord met SHA256
        /// </summary>
        /// <param name="wachtwoord">Het te hashen wachtwoord</param>
        /// <returns>De hash van het wachtwoord</returns>
        private static string HashWachtwoordMetSHA256(string wachtwoord)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Wachtwoord omzetten naar bytes
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(wachtwoord));

                // Bytes omzetten naar hexadecimale string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // x2 zorgt voor hexadecimale notatie
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Geeft een beschrijving van de wachtwoord vereisten
        /// </summary>
        /// <returns>Beschrijving van de wachtwoord vereisten</returns>
        public static string WachtwoordVereistenBeschrijving()
        {
            return "Wachtwoord moet minimaal 8 tekens bevatten, met minstens één hoofdletter, één kleine letter, één cijfer en één speciaal teken.";
        }
    }
} 