using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkTool.ClassLibrary.Data;
using BenchmarkTool.ClassLibrary.Models;

namespace BenchmarkTool.ClassLibrary.Services
{
    /// <summary>
    /// Service voor het beheren van jaarrapporten
    /// </summary>
    public class JaarrapportService
    {
        private readonly IJaarrapportRepository _jaarrapportRepository;

        /// <summary>
        /// Constructor voor JaarrapportService
        /// </summary>
        public JaarrapportService()
        {
            _jaarrapportRepository = new JaarrapportRepository();
        }

        /// <summary>
        /// Constructor met expliciete JaarrapportRepository voor testdoeleinden
        /// </summary>
        /// <param name="jaarrapportRepository">De te gebruiken JaarrapportRepository</param>
        public JaarrapportService(IJaarrapportRepository jaarrapportRepository)
        {
            _jaarrapportRepository = jaarrapportRepository;
        }

        /// <summary>
        /// Maakt een nieuw jaarrapport aan
        /// </summary>
        /// <param name="report">Het aan te maken jaarrapport</param>
        /// <returns>ID van het nieuwe jaarrapport of -1 bij een fout</returns>
        public int CreateYearreport(Jaarrapport report)
        {
            try
            {
                // Validatie
                if (report == null)
                {
                    throw new ArgumentNullException(nameof(report), "Jaarrapport mag niet null zijn.");
                }

                if (report.Year <= 0)
                {
                    throw new ArgumentException("Jaar moet groter zijn dan 0.", nameof(report));
                }

                if (report.CompanyId <= 0)
                {
                    throw new ArgumentException("Bedrijf ID is ongeldig.", nameof(report));
                }

                // Controleer of er al een jaarrapport bestaat voor dit jaar en bedrijf
                var bestaandRapport = _jaarrapportRepository.GetByYearAndCompany(report.Year, report.CompanyId);
                if (bestaandRapport != null)
                {
                    throw new InvalidOperationException($"Er bestaat al een jaarrapport voor bedrijf {report.CompanyId} en jaar {report.Year}.");
                }

                // Maak jaarrapport aan in de database
                return _jaarrapportRepository.Create(report);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij aanmaken jaarrapport: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Haalt alle jaarrapporten op voor een bedrijf
        /// </summary>
        /// <param name="companyId">ID van het bedrijf</param>
        /// <returns>Lijst met jaarrapporten of een lege lijst bij een fout</returns>
        public List<Jaarrapport> GetYearreportsByCompany(int companyId)
        {
            try
            {
                if (companyId <= 0)
                {
                    throw new ArgumentException("Bedrijf ID is ongeldig.", nameof(companyId));
                }

                return _jaarrapportRepository.GetByCompanyId(companyId);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen jaarrapporten voor bedrijf {companyId}: {ex.Message}");
                return new List<Jaarrapport>();
            }
        }

        /// <summary>
        /// Haalt gedetailleerde informatie op over een jaarrapport, inclusief kosten en antwoorden
        /// </summary>
        /// <param name="reportId">ID van het jaarrapport</param>
        /// <returns>Dictionary met jaarrapport details of null bij een fout</returns>
        public Dictionary<string, object> GetYearreportDetails(int reportId)
        {
            try
            {
                if (reportId <= 0)
                {
                    throw new ArgumentException("Jaarrapport ID is ongeldig.", nameof(reportId));
                }

                // Haal het jaarrapport op
                var jaarrapport = _jaarrapportRepository.GetById(reportId);
                if (jaarrapport == null)
                {
                    throw new InvalidOperationException($"Jaarrapport met ID {reportId} bestaat niet.");
                }

                // Haal kosten en antwoorden op
                var kosten = _jaarrapportRepository.GetCosts(reportId);
                var antwoorden = _jaarrapportRepository.GetAnswers(reportId);

                // Maak een dictionary met alle details
                var details = new Dictionary<string, object>
                {
                    ["Jaarrapport"] = jaarrapport,
                    ["Kosten"] = kosten,
                    ["Antwoorden"] = antwoorden
                };

                return details;
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen details jaarrapport {reportId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Werkt een bestaand jaarrapport bij
        /// </summary>
        /// <param name="report">Het bij te werken jaarrapport</param>
        /// <returns>True als het jaarrapport succesvol is bijgewerkt</returns>
        public bool UpdateYearreport(Jaarrapport report)
        {
            try
            {
                // Validatie
                if (report == null)
                {
                    throw new ArgumentNullException(nameof(report), "Jaarrapport mag niet null zijn.");
                }

                if (report.Id <= 0)
                {
                    throw new ArgumentException("Jaarrapport ID is ongeldig.", nameof(report));
                }

                if (report.Year <= 0)
                {
                    throw new ArgumentException("Jaar moet groter zijn dan 0.", nameof(report));
                }

                if (report.CompanyId <= 0)
                {
                    throw new ArgumentException("Bedrijf ID is ongeldig.", nameof(report));
                }

                // Controleer of het jaarrapport bestaat
                var bestaandRapport = _jaarrapportRepository.GetById(report.Id);
                if (bestaandRapport == null)
                {
                    throw new InvalidOperationException($"Jaarrapport met ID {report.Id} bestaat niet.");
                }

                // Controleer of er geen ander jaarrapport bestaat voor dit jaar en bedrijf
                if (bestaandRapport.Year != report.Year || bestaandRapport.CompanyId != report.CompanyId)
                {
                    var conflicterendRapport = _jaarrapportRepository.GetByYearAndCompany(report.Year, report.CompanyId);
                    if (conflicterendRapport != null && conflicterendRapport.Id != report.Id)
                    {
                        throw new InvalidOperationException($"Er bestaat al een jaarrapport voor bedrijf {report.CompanyId} en jaar {report.Year}.");
                    }
                }

                // Werk het jaarrapport bij in de database
                return _jaarrapportRepository.Update(report);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij bijwerken jaarrapport {report?.Id}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verwijdert een jaarrapport
        /// </summary>
        /// <param name="reportId">ID van het jaarrapport</param>
        /// <returns>True als het jaarrapport succesvol is verwijderd</returns>
        public bool DeleteYearreport(int reportId)
        {
            try
            {
                if (reportId <= 0)
                {
                    throw new ArgumentException("Jaarrapport ID is ongeldig.", nameof(reportId));
                }

                // Controleer of het jaarrapport bestaat
                var jaarrapport = _jaarrapportRepository.GetById(reportId);
                if (jaarrapport == null)
                {
                    throw new InvalidOperationException($"Jaarrapport met ID {reportId} bestaat niet.");
                }

                // Verwijder het jaarrapport
                return _jaarrapportRepository.Delete(reportId);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij verwijderen jaarrapport {reportId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Slaat kosten op voor een jaarrapport
        /// </summary>
        /// <param name="reportId">ID van het jaarrapport</param>
        /// <param name="costs">Lijst met kosten</param>
        /// <returns>Aantal succesvol opgeslagen kosten</returns>
        public int SaveCostsForReport(int reportId, List<Kost> costs)
        {
            try
            {
                if (reportId <= 0)
                {
                    throw new ArgumentException("Jaarrapport ID is ongeldig.", nameof(reportId));
                }

                if (costs == null)
                {
                    throw new ArgumentNullException(nameof(costs), "Kosten mogen niet null zijn.");
                }

                // Controleer of het jaarrapport bestaat
                var jaarrapport = _jaarrapportRepository.GetById(reportId);
                if (jaarrapport == null)
                {
                    throw new InvalidOperationException($"Jaarrapport met ID {reportId} bestaat niet.");
                }

                // Zorg ervoor dat alle kosten het juiste jaarrapport ID hebben
                foreach (var kost in costs)
                {
                    kost.YearreportId = reportId;
                }

                // Sla de kosten op
                return _jaarrapportRepository.SaveCosts(reportId, costs);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij opslaan kosten voor jaarrapport {reportId}: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Slaat antwoorden op voor een jaarrapport
        /// </summary>
        /// <param name="reportId">ID van het jaarrapport</param>
        /// <param name="answers">Lijst met antwoorden</param>
        /// <returns>Aantal succesvol opgeslagen antwoorden</returns>
        public int SaveAnswersForReport(int reportId, List<Antwoord> answers)
        {
            try
            {
                if (reportId <= 0)
                {
                    throw new ArgumentException("Jaarrapport ID is ongeldig.", nameof(reportId));
                }

                if (answers == null)
                {
                    throw new ArgumentNullException(nameof(answers), "Antwoorden mogen niet null zijn.");
                }

                // Controleer of het jaarrapport bestaat
                var jaarrapport = _jaarrapportRepository.GetById(reportId);
                if (jaarrapport == null)
                {
                    throw new InvalidOperationException($"Jaarrapport met ID {reportId} bestaat niet.");
                }

                // Haal alle vragen op die niet van het type 'info' zijn
                var relevantVragen = _jaarrapportRepository.GetQuestionsNotInfo();
                var relevantVraagIds = relevantVragen.Select(v => v.Id).ToList();

                // Filter de antwoorden op relevante vragen
                var filteredAnswers = answers
                    .Where(a => relevantVraagIds.Contains(a.QuestionId))
                    .ToList();

                // Zorg ervoor dat alle antwoorden het juiste jaarrapport ID hebben
                foreach (var antwoord in filteredAnswers)
                {
                    antwoord.YearreportId = reportId;
                }

                // Sla de antwoorden op
                return _jaarrapportRepository.SaveAnswers(reportId, filteredAnswers);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij opslaan antwoorden voor jaarrapport {reportId}: {ex.Message}");
                return 0;
            }
        }
    }
} 