using System;
using System.Security.Cryptography;
using System.Text;

namespace BenchmarkTool.ClassLibrary.Authentication
{
    /// <summary>
    /// Hulpklasse voor het testen van wachtwoord hashing en verificatie
    /// </summary>
    public static class WachtwoordTestHelper
    {
        /// <summary>
        /// Test de wachtwoord verificatie tussen een hash en een wachtwoord
        /// en toont het resultaat in de console.
        /// </summary>
        /// <param name="hash">De opgeslagen hash</param>
        /// <param name="wachtwoord">Het te controleren wachtwoord</param>
        public static void TestWachtwoordVerificatie(string hash, string wachtwoord)
        {
            try
            {
                // Hash het ingevoerde wachtwoord
                string hashVanIngevoerdWachtwoord = HashWachtwoord(wachtwoord);
                
                Console.WriteLine($"Opgeslagen hash: {hash}");
                Console.WriteLine($"Berekende hash: {hashVanIngevoerdWachtwoord}");
                Console.WriteLine($"Wachtwoord: {wachtwoord}");
                Console.WriteLine($"Hashes komen overeen: {string.Equals(hash, hashVanIngevoerdWachtwoord, StringComparison.OrdinalIgnoreCase)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij testen van wachtwoord verificatie: {ex.Message}");
                Console.WriteLine($"Directe vergelijking: {hash == wachtwoord}");
            }
        }

        /// <summary>
        /// Genereert een nieuwe hash voor het gegeven wachtwoord
        /// </summary>
        /// <param name="wachtwoord">Het wachtwoord om te hashen</param>
        /// <returns>De hash van het wachtwoord</returns>
        public static string GenereerNieuweHash(string wachtwoord)
        {
            return HashWachtwoord(wachtwoord);
        }

        /// <summary>
        /// Veilige verificatie methode die zowel hash verificatie probeert als directe vergelijking
        /// </summary>
        /// <param name="opgeslagenWaarde">De opgeslagen hash of wachtwoord</param>
        /// <param name="ingevoerdWachtwoord">Het ingevoerde wachtwoord</param>
        /// <returns>True als verificatie slaagt via een van beide methodes</returns>
        public static bool VeiligeVerificatie(string opgeslagenWaarde, string ingevoerdWachtwoord)
        {
            // Probeer eerst directe verificatie voor simpele gevallen
            if (opgeslagenWaarde == ingevoerdWachtwoord)
            {
                Console.WriteLine("Verificatie geslaagd via directe vergelijking");
                return true;
            }

            // Probeer daarna hash verificatie
            try
            {
                // Hash het ingevoerde wachtwoord
                string hashVanIngevoerdWachtwoord = HashWachtwoord(ingevoerdWachtwoord);
                
                // Vergelijk de hashes (case-insensitive vergelijking voor hexadecimale waardes)
                bool valid = string.Equals(opgeslagenWaarde, hashVanIngevoerdWachtwoord, StringComparison.OrdinalIgnoreCase);
                
                if (valid)
                {
                    Console.WriteLine("Verificatie geslaagd via hash verificatie");
                }
                
                return valid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hash verificatie mislukt: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Hasht een wachtwoord met SHA256
        /// </summary>
        /// <param name="wachtwoord">Wachtwoord om te hashen</param>
        /// <returns>Gehashte wachtwoord string</returns>
        private static string HashWachtwoord(string wachtwoord)
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
    }
} 