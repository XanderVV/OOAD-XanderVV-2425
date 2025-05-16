using System.Collections.Generic;
using BenchmarkTool.ClassLibrary.Models;

namespace BenchmarkTool.ClassLibrary.Data
{
    /// <summary>
    /// Interface voor repository klasse die benchmark data beheert
    /// </summary>
    public interface IBenchmarkRepository
    {
        /// <summary>
        /// Haalt benchmark data op van andere bedrijven op basis van criteria
        /// </summary>
        /// <param name="currentCompanyId">ID van het huidige bedrijf (om uit te sluiten)</param>
        /// <param name="year">Jaar waarvoor data opgehaald moet worden</param>
        /// <param name="naceFilter">Filter voor NACE-code (kan null zijn voor geen filter)</param>
        /// <param name="groupingLevel">Niveau van NACE-code groepering</param>
        /// <param name="selectedIndicators">Lijst van geselecteerde indicatoren (Questions.id, Costtypes.type, 'fte')</param>
        /// <returns>Lijst van benchmark data</returns>
        List<BenchmarkData> GetBenchmarkData(
            int currentCompanyId,
            int year,
            string naceFilter,
            NaceGroupingLevel groupingLevel,
            List<string> selectedIndicators);

        /// <summary>
        /// Haalt eigen data op van een bedrijf voor vergelijking met de benchmark
        /// </summary>
        /// <param name="companyId">ID van het bedrijf</param>
        /// <param name="year">Jaar waarvoor data opgehaald moet worden</param>
        /// <param name="selectedIndicators">Lijst van geselecteerde indicatoren (Questions.id, Costtypes.type, 'fte')</param>
        /// <returns>Lijst van eigen data</returns>
        List<BenchmarkData> GetOwnData(
            int companyId,
            int year,
            List<string> selectedIndicators);
    }
} 