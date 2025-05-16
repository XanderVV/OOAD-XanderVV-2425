using System.Configuration;
using BenchmarkTool.ClassLibrary.Data;
using BenchmarkTool.ClassLibrary.Models;

namespace BenchmarkTool.ClassLibrary.Authentication
{
    /// <summary>
    /// Service voor authenticatie en autorisatie van gebruikers (admins en bedrijven)
    /// </summary>
    public class AuthenticatieService
    {
        private readonly WachtwoordHasher _wachtwoordHasher;
        private readonly BedrijfRepository _bedrijfRepository;

        public AuthenticatieService()
        {
            _wachtwoordHasher = new WachtwoordHasher();
            _bedrijfRepository = new BedrijfRepository();
        }

        /// <summary>
        /// Valideert admin inloggegevens tegen de configuratie
        /// </summary>
        /// <param name="gebruikersnaam">Admin gebruikersnaam</param>
        /// <param name="wachtwoord">Admin wachtwoord</param>
        /// <returns>True als de inloggegevens geldig zijn, anders False</returns>
        public bool ValideerAdminCredentials(string gebruikersnaam, string wachtwoord)
        {
            try
            {
                // Haal admin gegevens op uit App.config
                var configGebruikersnaam = ConfigurationManager.AppSettings["AdminGebruikersnaam"];
                var configWachtwoordHash = ConfigurationManager.AppSettings["AdminWachtwoordHash"];

                // Controleer of de gegevens uit config bestaan
                if (string.IsNullOrEmpty(configGebruikersnaam) || string.IsNullOrEmpty(configWachtwoordHash))
                {
                    return false;
                }

                // Controleer gebruikersnaam
                if (gebruikersnaam != configGebruikersnaam)
                {
                    return false;
                }

                // Controleer wachtwoord tegen de SHA256 hash
                return _wachtwoordHasher.VerifieerWachtwoord(configWachtwoordHash, wachtwoord);
            }
            catch (Exception ex)
            {
                // Log de fout
                Console.WriteLine($"Fout bij admin authenticatie: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Valideert bedrijfsinloggegevens tegen de database
        /// </summary>
        /// <param name="gebruikersnaam">Bedrijf gebruikersnaam (login)</param>
        /// <param name="wachtwoord">Bedrijf wachtwoord</param>
        /// <returns>Bedrijfsobject als login succesvol is, anders null</returns>
        public Bedrijf? ValideerBedrijfsCredentials(string gebruikersnaam, string wachtwoord)
        {
            try
            {
                // Haal het bedrijf op basis van login
                var bedrijf = _bedrijfRepository.GetBedrijfByLogin(gebruikersnaam);

                // Controleer of het bedrijf bestaat
                if (bedrijf == null)
                {
                    return null;
                }

                // Controleer of het bedrijf actief is
                if (bedrijf.Status != "Active")
                {
                    return null;
                }

                // Controleer het wachtwoord tegen de hash
                if (string.IsNullOrEmpty(bedrijf.Password))
                {
                    return null;
                }

                if (_wachtwoordHasher.VerifieerWachtwoord(bedrijf.Password, wachtwoord))
                {
                    return bedrijf;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log de fout
                Console.WriteLine($"Fout bij bedrijfsauthenticatie: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Hasht een wachtwoord voor opslag
        /// </summary>
        /// <param name="wachtwoord">Het te hashen wachtwoord</param>
        /// <returns>De hash van het wachtwoord</returns>
        public string HashPassword(string wachtwoord)
        {
            return _wachtwoordHasher.HashWachtwoord(wachtwoord);
        }

        /// <summary>
        /// Verifieert een wachtwoord tegen een hash
        /// </summary>
        /// <param name="hashedWachtwoord">De opgeslagen hash</param>
        /// <param name="providedWachtwoord">Het ingevoerde wachtwoord</param>
        /// <returns>True als wachtwoord correct is</returns>
        public bool VerifyPassword(string hashedWachtwoord, string providedWachtwoord)
        {
            return _wachtwoordHasher.VerifieerWachtwoord(hashedWachtwoord, providedWachtwoord);
        }

        /// <summary>
        /// Genereert een veilig willekeurig wachtwoord
        /// </summary>
        /// <param name="lengte">De lengte van het wachtwoord</param>
        /// <returns>Het gegenereerde wachtwoord</returns>
        public string GenereerWachtwoord(int lengte = 12)
        {
            return _wachtwoordHasher.GenereerWillekeurigWachtwoord(lengte);
        }
    }
} 