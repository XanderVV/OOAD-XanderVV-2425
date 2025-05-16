using System;
using System.Security.Cryptography;
using System.Text;

namespace BenchmarkTool.ClassLibrary.Authentication
{
    /// <summary>
    /// Klasse voor het veilig hashen en verifiëren van wachtwoorden met SHA256
    /// </summary>
    public class WachtwoordHasher
    {
        /// <summary>
        /// Hasht een wachtwoord met SHA256
        /// </summary>
        /// <param name="wachtwoord">Het te hashen wachtwoord</param>
        /// <returns>De gehashte wachtwoord string (hexadecimaal)</returns>
        public string HashWachtwoord(string wachtwoord)
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
        /// Verifieert een wachtwoord tegen een opgeslagen SHA256 hash
        /// </summary>
        /// <param name="opgeslagenHash">De opgeslagen hash</param>
        /// <param name="ingevoerdWachtwoord">Het ingevoerde wachtwoord</param>
        /// <returns>True als het wachtwoord overeenkomt met de hash</returns>
        public bool VerifieerWachtwoord(string opgeslagenHash, string ingevoerdWachtwoord)
        {
            // Hash het ingevoerde wachtwoord
            string hashVanIngevoerdWachtwoord = HashWachtwoord(ingevoerdWachtwoord);
            
            // Vergelijk de hashes (case-insensitive vergelijking voor hexadecimale waardes)
            return string.Equals(opgeslagenHash, hashVanIngevoerdWachtwoord, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Genereert een veilig, willekeurig wachtwoord
        /// </summary>
        /// <param name="lengte">De lengte van het wachtwoord</param>
        /// <returns>Een willekeurig wachtwoord</returns>
        public string GenereerWillekeurigWachtwoord(int lengte = 12)
        {
            return WachtwoordHelper.GenereerWillekeurigWachtwoord(lengte);
        }

        /// <summary>
        /// Valideert of een wachtwoord voldoet aan de complexiteitseisen
        /// </summary>
        /// <param name="wachtwoord">Het te valideren wachtwoord</param>
        /// <returns>True als het wachtwoord voldoet aan de eisen</returns>
        public bool ValideerWachtwoordComplexiteit(string wachtwoord)
        {
            return AuthenticatieHelper.ValideerWachtwoordComplexiteit(wachtwoord);
        }
    }
} 