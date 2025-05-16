using System;
using System.Security.Cryptography;
using System.Text;

namespace BenchmarkTool.ClassLibrary.Authentication
{
    /// <summary>
    /// Helper klasse voor wachtwoordfunctionaliteit
    /// </summary>
    public static class WachtwoordHelper
    {
        /// <summary>
        /// Hasht een wachtwoord met SHA256
        /// </summary>
        /// <param name="wachtwoord">Wachtwoord om te hashen</param>
        /// <returns>Gehashte wachtwoord string</returns>
        public static string HashWachtwoord(string wachtwoord)
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
        /// Verifieert een wachtwoord tegen een hash
        /// </summary>
        /// <param name="opgeslagenHash">De hash waartegen geverifieerd wordt</param>
        /// <param name="ingevoerdWachtwoord">Het te controleren wachtwoord</param>
        /// <returns>True als het wachtwoord overeenkomt met de hash</returns>
        public static bool VerifieerWachtwoord(string opgeslagenHash, string ingevoerdWachtwoord)
        {
            // Hash het ingevoerde wachtwoord
            string hashVanIngevoerdWachtwoord = HashWachtwoord(ingevoerdWachtwoord);
            
            // Vergelijk de hashes (case-insensitive vergelijking voor hexadecimale waardes)
            return string.Equals(opgeslagenHash, hashVanIngevoerdWachtwoord, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Genereert een veilig willekeurig wachtwoord
        /// </summary>
        /// <param name="lengte">De lengte van het wachtwoord (default 12)</param>
        /// <returns>Een willekeurig gegenereerd wachtwoord</returns>
        public static string GenereerWillekeurigWachtwoord(int lengte = 12)
        {
            const string hoofdletters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string kleineletters = "abcdefghijkmnopqrstuvwxyz";
            const string cijfers = "0123456789";
            const string specialeTekens = "!@#$%^&*()_-+=";

            var random = new Random();
            var tekens = new char[lengte];

            // Zorg voor minimaal één van elk type
            tekens[0] = hoofdletters[random.Next(hoofdletters.Length)];
            tekens[1] = kleineletters[random.Next(kleineletters.Length)];
            tekens[2] = cijfers[random.Next(cijfers.Length)];
            tekens[3] = specialeTekens[random.Next(specialeTekens.Length)];

            // Vul de rest van het wachtwoord aan met willekeurige tekens
            var alleKarakters = hoofdletters + kleineletters + cijfers + specialeTekens;
            for (int i = 4; i < lengte; i++)
            {
                tekens[i] = alleKarakters[random.Next(alleKarakters.Length)];
            }

            // Schud de tekens door elkaar
            for (int i = 0; i < lengte; i++)
            {
                int j = random.Next(lengte);
                (tekens[i], tekens[j]) = (tekens[j], tekens[i]);
            }

            return new string(tekens);
        }
    }
} 