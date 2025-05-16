using System;
using System.Collections.Generic;
using BenchmarkTool.ClassLibrary.Authentication;
using BenchmarkTool.ClassLibrary.Data;
using BenchmarkTool.ClassLibrary.Models;

namespace BenchmarkTool.ClassLibrary.Services
{
    /// <summary>
    /// Service voor het beheren van bedrijven
    /// </summary>
    public class BedrijfService
    {
        private readonly BedrijfRepository _bedrijfRepository;
        private readonly WachtwoordHasher _wachtwoordHasher;

        /// <summary>
        /// Constructor voor BedrijfService
        /// </summary>
        public BedrijfService()
        {
            _bedrijfRepository = new BedrijfRepository();
            _wachtwoordHasher = new WachtwoordHasher();
        }

        /// <summary>
        /// Constructor met expliciete BedrijfRepository voor testdoeleinden
        /// </summary>
        /// <param name="bedrijfRepository">De te gebruiken BedrijfRepository</param>
        public BedrijfService(BedrijfRepository bedrijfRepository)
        {
            _bedrijfRepository = bedrijfRepository;
            _wachtwoordHasher = new WachtwoordHasher();
        }

        /// <summary>
        /// Constructor met expliciete BedrijfRepository en WachtwoordHasher voor testdoeleinden
        /// </summary>
        /// <param name="bedrijfRepository">De te gebruiken BedrijfRepository</param>
        /// <param name="wachtwoordHasher">De te gebruiken WachtwoordHasher</param>
        public BedrijfService(BedrijfRepository bedrijfRepository, WachtwoordHasher wachtwoordHasher)
        {
            _bedrijfRepository = bedrijfRepository;
            _wachtwoordHasher = wachtwoordHasher;
        }

        /// <summary>
        /// Voegt een nieuw bedrijf toe aan de database
        /// </summary>
        /// <param name="bedrijf">Het toe te voegen bedrijf</param>
        /// <returns>ID van het nieuwe bedrijf of -1 bij een fout</returns>
        public int CreateCompany(Bedrijf bedrijf)
        {
            try
            {
                // Validatie
                if (bedrijf == null)
                {
                    throw new ArgumentNullException(nameof(bedrijf), "Bedrijf mag niet null zijn.");
                }

                if (string.IsNullOrEmpty(bedrijf.Name))
                {
                    throw new ArgumentException("Naam is verplicht.", nameof(bedrijf));
                }

                if (string.IsNullOrEmpty(bedrijf.Login))
                {
                    throw new ArgumentException("Login is verplicht.", nameof(bedrijf));
                }

                // Controleer of login al bestaat
                var bestaandBedrijf = _bedrijfRepository.GetBedrijfByLogin(bedrijf.Login);
                if (bestaandBedrijf != null)
                {
                    throw new InvalidOperationException($"Een bedrijf met login '{bedrijf.Login}' bestaat al.");
                }

                // Zorg ervoor dat de registratiedatum is ingesteld
                if (bedrijf.RegDate == default)
                {
                    bedrijf.RegDate = DateTime.Now;
                }

                // Maak bedrijf aan in de database
                return _bedrijfRepository.Create(bedrijf);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij aanmaken bedrijf: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Haalt een bedrijf op basis van ID
        /// </summary>
        /// <param name="id">ID van het bedrijf</param>
        /// <returns>Het gevonden bedrijf of null</returns>
        public Bedrijf GetCompanyById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("ID moet groter zijn dan 0.", nameof(id));
                }

                return _bedrijfRepository.GetById(id);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen bedrijf met ID {id}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Haalt een bedrijf op basis van gebruikersnaam (login)
        /// </summary>
        /// <param name="username">Gebruikersnaam van het bedrijf</param>
        /// <returns>Het gevonden bedrijf of null</returns>
        public Bedrijf GetCompanyByUsername(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    throw new ArgumentException("Gebruikersnaam mag niet leeg zijn.", nameof(username));
                }

                return _bedrijfRepository.GetBedrijfByLogin(username);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen bedrijf met gebruikersnaam {username}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Haalt alle bedrijven op
        /// </summary>
        /// <returns>Lijst van alle bedrijven of een lege lijst bij een fout</returns>
        public List<Bedrijf> GetAllCompanies()
        {
            try
            {
                return _bedrijfRepository.GetAll();
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen van alle bedrijven: {ex.Message}");
                return new List<Bedrijf>();
            }
        }

        /// <summary>
        /// Werkt een bestaand bedrijf bij in de database
        /// </summary>
        /// <param name="bedrijf">Het bij te werken bedrijf</param>
        /// <returns>True als het bedrijf succesvol is bijgewerkt</returns>
        public bool UpdateCompany(Bedrijf bedrijf)
        {
            try
            {
                // Validatie
                if (bedrijf == null)
                {
                    throw new ArgumentNullException(nameof(bedrijf), "Bedrijf mag niet null zijn.");
                }

                if (bedrijf.Id <= 0)
                {
                    throw new ArgumentException("Bedrijf ID is ongeldig.", nameof(bedrijf));
                }

                if (string.IsNullOrEmpty(bedrijf.Name))
                {
                    throw new ArgumentException("Naam is verplicht.", nameof(bedrijf));
                }

                if (string.IsNullOrEmpty(bedrijf.Login))
                {
                    throw new ArgumentException("Login is verplicht.", nameof(bedrijf));
                }

                // Controleer of het bedrijf bestaat
                var bestaandBedrijf = _bedrijfRepository.GetById(bedrijf.Id);
                if (bestaandBedrijf == null)
                {
                    throw new InvalidOperationException($"Bedrijf met ID {bedrijf.Id} bestaat niet.");
                }

                // Controleer of de nieuwe login niet al in gebruik is door een ander bedrijf
                if (bestaandBedrijf.Login != bedrijf.Login)
                {
                    var bedrijfMetLogin = _bedrijfRepository.GetBedrijfByLogin(bedrijf.Login);
                    if (bedrijfMetLogin != null && bedrijfMetLogin.Id != bedrijf.Id)
                    {
                        throw new InvalidOperationException($"Een bedrijf met login '{bedrijf.Login}' bestaat al.");
                    }
                }

                // Zorg ervoor dat de LastModified datum is ingesteld
                bedrijf.LastModified = DateTime.Now;

                // Werk het bedrijf bij in de database
                return _bedrijfRepository.Update(bedrijf);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij bijwerken bedrijf met ID {bedrijf?.Id}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verwijdert een bedrijf uit de database
        /// </summary>
        /// <param name="id">ID van het te verwijderen bedrijf</param>
        /// <returns>True als het bedrijf succesvol is verwijderd</returns>
        public bool DeleteCompany(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("ID moet groter zijn dan 0.", nameof(id));
                }

                // Controleer of het bedrijf bestaat
                var bedrijf = _bedrijfRepository.GetById(id);
                if (bedrijf == null)
                {
                    throw new InvalidOperationException($"Bedrijf met ID {id} bestaat niet.");
                }

                // Verwijder het bedrijf
                return _bedrijfRepository.Delete(id);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij verwijderen bedrijf met ID {id}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Haalt alle bedrijven op met status 'Pending'
        /// </summary>
        /// <returns>Lijst van bedrijven met status 'Pending' of een lege lijst bij een fout</returns>
        public List<Bedrijf> GetPendingRegistrations()
        {
            try
            {
                return _bedrijfRepository.GetPendingRegistrations();
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen van bedrijven met status 'Pending': {ex.Message}");
                return new List<Bedrijf>();
            }
        }

        /// <summary>
        /// Keurt een bedrijfsregistratie goed en stelt het initiële wachtwoord in
        /// </summary>
        /// <param name="companyId">ID van het bedrijf</param>
        /// <param name="initialPassword">Het initiële wachtwoord</param>
        /// <returns>True als de goedkeuring succesvol is</returns>
        public bool ApproveRegistration(int companyId, string initialPassword)
        {
            try
            {
                if (companyId <= 0)
                {
                    throw new ArgumentException("Bedrijf ID is ongeldig.", nameof(companyId));
                }

                if (string.IsNullOrEmpty(initialPassword))
                {
                    throw new ArgumentException("Het initiële wachtwoord mag niet leeg zijn.", nameof(initialPassword));
                }

                // Haal het bedrijf op
                var bedrijf = _bedrijfRepository.GetById(companyId);
                if (bedrijf == null)
                {
                    throw new InvalidOperationException($"Bedrijf met ID {companyId} bestaat niet.");
                }

                // Controleer of het bedrijf de status 'Pending' heeft
                if (bedrijf.Status != "Pending")
                {
                    throw new InvalidOperationException($"Bedrijf met ID {companyId} heeft niet de status 'Pending'.");
                }

                // Hash het wachtwoord
                string hashedPassword = _wachtwoordHasher.HashWachtwoord(initialPassword);

                // Werk het bedrijf bij
                bedrijf.Password = hashedPassword;
                bedrijf.Status = "Active";
                bedrijf.AcceptDate = DateTime.Now;
                bedrijf.LastModified = DateTime.Now;

                // Sla de wijzigingen op
                return _bedrijfRepository.Update(bedrijf);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij goedkeuren van bedrijfsregistratie met ID {companyId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Wijst een bedrijfsregistratie af
        /// </summary>
        /// <param name="companyId">ID van het bedrijf</param>
        /// <returns>True als de afwijzing succesvol is</returns>
        public bool RejectRegistration(int companyId)
        {
            try
            {
                if (companyId <= 0)
                {
                    throw new ArgumentException("Bedrijf ID is ongeldig.", nameof(companyId));
                }

                // Haal het bedrijf op
                var bedrijf = _bedrijfRepository.GetById(companyId);
                if (bedrijf == null)
                {
                    throw new InvalidOperationException($"Bedrijf met ID {companyId} bestaat niet.");
                }

                // Controleer of het bedrijf de status 'Pending' heeft
                if (bedrijf.Status != "Pending")
                {
                    throw new InvalidOperationException($"Bedrijf met ID {companyId} heeft niet de status 'Pending'.");
                }

                // Werk het bedrijf bij
                bedrijf.Status = "Rejected";
                bedrijf.LastModified = DateTime.Now;

                // Sla de wijzigingen op
                return _bedrijfRepository.Update(bedrijf);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij afwijzen van bedrijfsregistratie met ID {companyId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Haalt het logo van een bedrijf op
        /// </summary>
        /// <param name="companyId">ID van het bedrijf</param>
        /// <returns>Het logo als byte[] of null</returns>
        public byte[] GetCompanyLogo(int companyId)
        {
            try
            {
                if (companyId <= 0)
                {
                    throw new ArgumentException("Bedrijf ID is ongeldig.", nameof(companyId));
                }

                return _bedrijfRepository.GetLogo(companyId);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen logo van bedrijf met ID {companyId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Werkt het logo van een bedrijf bij
        /// </summary>
        /// <param name="companyId">ID van het bedrijf</param>
        /// <param name="logo">Het nieuwe logo als byte[]</param>
        /// <returns>True als het logo succesvol is bijgewerkt</returns>
        public bool UpdateCompanyLogo(int companyId, byte[] logo)
        {
            try
            {
                if (companyId <= 0)
                {
                    throw new ArgumentException("Bedrijf ID is ongeldig.", nameof(companyId));
                }

                if (logo == null || logo.Length == 0)
                {
                    throw new ArgumentException("Logo mag niet leeg zijn.", nameof(logo));
                }

                // Controleer of het bedrijf bestaat
                var bedrijf = _bedrijfRepository.GetById(companyId);
                if (bedrijf == null)
                {
                    throw new InvalidOperationException($"Bedrijf met ID {companyId} bestaat niet.");
                }

                // Werk het logo bij
                return _bedrijfRepository.UpdateLogo(companyId, logo);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij bijwerken logo van bedrijf met ID {companyId}: {ex.Message}");
                return false;
            }
        }

        #region Stamgegevens Ophalen

        /// <summary>
        /// Haalt alle NACE-codes op
        /// </summary>
        /// <returns>Lijst met alle NACE-codes</returns>
        public virtual List<Nacecode> GetAllNacecodes()
        {
            var repository = new DatabaseRepository();
            return repository.GetAllNacecodes();
        }

        /// <summary>
        /// Haalt alle categorieën op
        /// </summary>
        /// <returns>Lijst met alle categorieën</returns>
        public virtual List<Categorie> GetAllCategories()
        {
            var repository = new DatabaseRepository();
            return repository.GetAllCategories();
        }

        /// <summary>
        /// Haalt alle vragen op
        /// </summary>
        /// <returns>Lijst met alle vragen</returns>
        public virtual List<Vraag> GetAllQuestions()
        {
            var repository = new DatabaseRepository();
            return repository.GetAllQuestions();
        }

        /// <summary>
        /// Haalt alle kosttypes op
        /// </summary>
        /// <returns>Lijst met alle kosttypes</returns>
        public virtual List<KostType> GetAllCosttypes()
        {
            var repository = new DatabaseRepository();
            return repository.GetAllCosttypes();
        }

        #endregion
    }
} 