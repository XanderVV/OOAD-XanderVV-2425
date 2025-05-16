using System.Collections.Generic;
using BenchmarkTool.ClassLibrary.Models;

namespace BenchmarkTool.ClassLibrary.Data
{
    /// <summary>
    /// Interface voor repository klasse die jaarrapporten beheert
    /// </summary>
    public interface IJaarrapportRepository
    {
        /// <summary>
        /// Haalt een jaarrapport op basis van ID
        /// </summary>
        /// <param name="id">ID van het jaarrapport</param>
        /// <returns>Het gevonden jaarrapport of null</returns>
        Jaarrapport GetById(int id);

        /// <summary>
        /// Haalt alle jaarrapporten op voor een specifiek bedrijf
        /// </summary>
        /// <param name="companyId">ID van het bedrijf</param>
        /// <returns>Lijst van jaarrapporten voor het bedrijf</returns>
        List<Jaarrapport> GetByCompanyId(int companyId);

        /// <summary>
        /// Haalt een jaarrapport op basis van jaar en bedrijfs-ID
        /// </summary>
        /// <param name="year">Jaar van het rapport</param>
        /// <param name="companyId">ID van het bedrijf</param>
        /// <returns>Het gevonden jaarrapport of null</returns>
        Jaarrapport GetByYearAndCompany(int year, int companyId);

        /// <summary>
        /// Voegt een nieuw jaarrapport toe aan de database
        /// </summary>
        /// <param name="jaarrapport">Het toe te voegen jaarrapport</param>
        /// <returns>ID van het nieuwe jaarrapport</returns>
        int Create(Jaarrapport jaarrapport);

        /// <summary>
        /// Werkt een bestaand jaarrapport bij in de database
        /// </summary>
        /// <param name="jaarrapport">Het bij te werken jaarrapport</param>
        /// <returns>True als het jaarrapport succesvol is bijgewerkt</returns>
        bool Update(Jaarrapport jaarrapport);

        /// <summary>
        /// Verwijdert een jaarrapport uit de database
        /// </summary>
        /// <param name="id">ID van het te verwijderen jaarrapport</param>
        /// <returns>True als het jaarrapport succesvol is verwijderd</returns>
        bool Delete(int id);

        /// <summary>
        /// Haalt alle kosten op voor een specifiek jaarrapport
        /// </summary>
        /// <param name="yearreportId">ID van het jaarrapport</param>
        /// <returns>Lijst van kosten voor het jaarrapport</returns>
        List<Kost> GetCosts(int yearreportId);

        /// <summary>
        /// Haalt alle antwoorden op voor een specifiek jaarrapport
        /// </summary>
        /// <param name="yearreportId">ID van het jaarrapport</param>
        /// <returns>Lijst van antwoorden voor het jaarrapport</returns>
        List<Antwoord> GetAnswers(int yearreportId);

        /// <summary>
        /// Slaat kosten op voor een jaarrapport
        /// </summary>
        /// <param name="yearreportId">ID van het jaarrapport</param>
        /// <param name="costs">Lijst van kosten</param>
        /// <returns>Aantal succesvol opgeslagen kosten</returns>
        int SaveCosts(int yearreportId, List<Kost> costs);

        /// <summary>
        /// Slaat antwoorden op voor een jaarrapport
        /// </summary>
        /// <param name="yearreportId">ID van het jaarrapport</param>
        /// <param name="answers">Lijst van antwoorden</param>
        /// <returns>Aantal succesvol opgeslagen antwoorden</returns>
        int SaveAnswers(int yearreportId, List<Antwoord> answers);

        /// <summary>
        /// Haalt alle vragen op die niet van het type 'info' zijn
        /// </summary>
        /// <returns>Lijst van vragen</returns>
        List<Vraag> GetQuestionsNotInfo();
    }
} 