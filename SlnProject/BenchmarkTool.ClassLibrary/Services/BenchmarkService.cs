using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkTool.ClassLibrary.Data;
using BenchmarkTool.ClassLibrary.Models;

namespace BenchmarkTool.ClassLibrary.Services
{
    /// <summary>
    /// Service voor het beheren van benchmark operaties
    /// </summary>
    public class BenchmarkService
    {
        private readonly IBenchmarkRepository _benchmarkRepository;

        /// <summary>
        /// Constructor voor BenchmarkService
        /// </summary>
        public BenchmarkService()
        {
            _benchmarkRepository = new BenchmarkRepository();
        }

        /// <summary>
        /// Constructor met expliciete BenchmarkRepository voor testdoeleinden
        /// </summary>
        /// <param name="benchmarkRepository">De te gebruiken BenchmarkRepository</param>
        public BenchmarkService(IBenchmarkRepository benchmarkRepository)
        {
            _benchmarkRepository = benchmarkRepository;
        }

        /// <summary>
        /// Haalt benchmark data op voor vergelijking
        /// </summary>
        /// <param name="currentCompanyId">ID van het huidige bedrijf</param>
        /// <param name="year">Jaar waarvoor data opgehaald moet worden</param>
        /// <param name="naceFilter">Filter voor NACE-code (kan null zijn voor geen filter)</param>
        /// <param name="groupingLevel">Niveau van NACE-code groepering</param>
        /// <param name="selectedIndicators">Lijst van geselecteerde indicatoren</param>
        /// <returns>BenchmarkResultaat object met benchmark en eigen data</returns>
        public BenchmarkResultaat GetBenchmarkData(
            int currentCompanyId,
            int year,
            string naceFilter,
            NaceGroupingLevel groupingLevel,
            List<string> selectedIndicators)
        {
            try
            {
                // Validatie van input parameters
                if (currentCompanyId <= 0)
                {
                    throw new ArgumentException("Bedrijf ID is ongeldig.", nameof(currentCompanyId));
                }

                if (year <= 0)
                {
                    throw new ArgumentException("Jaar is ongeldig.", nameof(year));
                }

                if (selectedIndicators == null || !selectedIndicators.Any())
                {
                    throw new ArgumentException("Minstens één indicator moet geselecteerd zijn.", nameof(selectedIndicators));
                }

                // Ophalen van benchmark data (andere bedrijven)
                var benchmarkData = _benchmarkRepository.GetBenchmarkData(
                    currentCompanyId,
                    year,
                    naceFilter,
                    groupingLevel,
                    selectedIndicators);

                // Ophalen van eigen data voor vergelijking
                var ownData = _benchmarkRepository.GetOwnData(
                    currentCompanyId,
                    year,
                    selectedIndicators);

                return new BenchmarkResultaat
                {
                    BenchmarkGegevens = benchmarkData,
                    EigenGegevens = ownData
                };
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen benchmark data: {ex.Message}");
                return new BenchmarkResultaat();
            }
        }

        /// <summary>
        /// Berekent statistieken voor een indicator uit de benchmark data
        /// </summary>
        /// <param name="benchmarkData">Lijst van benchmark data</param>
        /// <param name="indicator">De indicator waarvoor statistieken berekend moeten worden</param>
        /// <returns>Dictionary met statistieken (gemiddelde, mediaan, etc.)</returns>
        public Dictionary<string, decimal> BerekenStatistieken(List<BenchmarkData> benchmarkData, string indicator)
        {
            var statistieken = new Dictionary<string, decimal>();

            try
            {
                // Filter data voor de specifieke indicator
                var indicatorData = benchmarkData
                    .Where(d => d.Indicator == indicator)
                    .Select(d => d.Waarde)
                    .ToList();

                if (!indicatorData.Any())
                {
                    return statistieken;
                }

                // Bereken gemiddelde
                statistieken["Gemiddelde"] = indicatorData.Average();

                // Bereken mediaan
                var gesorteerdeData = indicatorData.OrderBy(v => v).ToList();
                int middenIndex = gesorteerdeData.Count / 2;

                if (gesorteerdeData.Count % 2 == 0)
                {
                    statistieken["Mediaan"] = (gesorteerdeData[middenIndex - 1] + gesorteerdeData[middenIndex]) / 2;
                }
                else
                {
                    statistieken["Mediaan"] = gesorteerdeData[middenIndex];
                }

                // Bereken minimum en maximum
                statistieken["Minimum"] = indicatorData.Min();
                statistieken["Maximum"] = indicatorData.Max();

                // Bereken standaardafwijking
                double gemiddelde = (double)statistieken["Gemiddelde"];
                double somKwadratenVerschillen = indicatorData.Sum(v => Math.Pow((double)v - gemiddelde, 2));
                double variantie = somKwadratenVerschillen / indicatorData.Count;
                statistieken["Standaardafwijking"] = (decimal)Math.Sqrt(variantie);

                // Bereken percentielen (25%, 75%)
                int p25Index = (int)Math.Ceiling(gesorteerdeData.Count * 0.25) - 1;
                int p75Index = (int)Math.Ceiling(gesorteerdeData.Count * 0.75) - 1;

                statistieken["Percentiel25"] = gesorteerdeData[p25Index];
                statistieken["Percentiel75"] = gesorteerdeData[p75Index];
                
                // Aantal waarnemingen
                statistieken["AantalWaarnemingen"] = indicatorData.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij berekenen statistieken: {ex.Message}");
            }

            return statistieken;
        }

        /// <summary>
        /// Genereert een tekstueel rapport met sterke en zwakke punten op basis van benchmark vergelijking
        /// </summary>
        /// <param name="benchmarkResultaat">BenchmarkResultaat object met benchmark en eigen data</param>
        /// <returns>Tekstueel rapport met sterke en zwakke punten</returns>
        public string GenereerRapport(BenchmarkResultaat benchmarkResultaat)
        {
            try
            {
                if (benchmarkResultaat == null || 
                    benchmarkResultaat.BenchmarkGegevens == null || 
                    !benchmarkResultaat.BenchmarkGegevens.Any() ||
                    benchmarkResultaat.EigenGegevens == null ||
                    !benchmarkResultaat.EigenGegevens.Any())
                {
                    return "Er zijn onvoldoende gegevens beschikbaar om een rapport te genereren.";
                }

                // Verzamel unieke indicatoren
                var indicators = benchmarkResultaat.EigenGegevens
                    .Select(d => d.Indicator)
                    .Distinct()
                    .ToList();

                var rapportBuilder = new System.Text.StringBuilder();
                rapportBuilder.AppendLine("BENCHMARK RAPPORT - STERKE EN ZWAKKE PUNTEN");
                rapportBuilder.AppendLine("===========================================");
                rapportBuilder.AppendLine();

                foreach (var indicator in indicators)
                {
                    // Bereken statistieken voor deze indicator
                    var stats = BerekenStatistieken(benchmarkResultaat.BenchmarkGegevens, indicator);
                    
                    if (!stats.Any())
                    {
                        continue; // Geen benchmark data voor deze indicator
                    }

                    // Haal eigen waarde op
                    var eigenWaarde = benchmarkResultaat.EigenGegevens
                        .FirstOrDefault(d => d.Indicator == indicator)?.Waarde ?? 0;

                    // Bepaal indicator naam voor weergave
                    string indicatorNaam = indicator;
                    if (indicator.StartsWith("cost_"))
                    {
                        indicatorNaam = $"Kosten - {indicator.Substring(5)}";
                    }
                    else if (indicator.StartsWith("question_"))
                    {
                        indicatorNaam = $"Vraag {indicator.Substring(9)}";
                    }

                    rapportBuilder.AppendLine($"INDICATOR: {indicatorNaam}");
                    rapportBuilder.AppendLine($"Eigen waarde: {eigenWaarde:N2}");
                    rapportBuilder.AppendLine($"Benchmark gemiddelde: {stats["Gemiddelde"]:N2}");
                    rapportBuilder.AppendLine($"Benchmark mediaan: {stats["Mediaan"]:N2}");
                    rapportBuilder.AppendLine();

                    // Bepaal afwijking
                    decimal procentueleAfwijking = 0;
                    if (stats["Gemiddelde"] != 0)
                    {
                        procentueleAfwijking = (eigenWaarde - stats["Gemiddelde"]) / stats["Gemiddelde"] * 100;
                    }

                    // Verlaag de drempel naar 1% voor testdoeleinden (was 15%)
                    bool isSignificant = Math.Abs(procentueleAfwijking) > 1; 
                    
                    if (isSignificant)
                    {
                        // Interpreteer afwijking afhankelijk van indicator type
                        bool isLagerBeter = indicator.StartsWith("cost_"); // Voor kosten is lager beter

                        if ((isLagerBeter && eigenWaarde < stats["Gemiddelde"]) || 
                            (!isLagerBeter && eigenWaarde > stats["Gemiddelde"]))
                        {
                            rapportBuilder.AppendLine($"STERK PUNT: Uw waarde wijkt {Math.Abs(procentueleAfwijking):N1}% " +
                                $"{(isLagerBeter ? "gunstig af" : "positief af")} van het gemiddelde.");
                        }
                        else
                        {
                            rapportBuilder.AppendLine($"AANDACHTSPUNT: Uw waarde wijkt {Math.Abs(procentueleAfwijking):N1}% " +
                                $"{(isLagerBeter ? "ongunstig af" : "negatief af")} van het gemiddelde.");
                        }
                    }
                    else
                    {
                        rapportBuilder.AppendLine("Uw waarde ligt dicht bij het benchmark gemiddelde.");
                    }

                    rapportBuilder.AppendLine();
                    rapportBuilder.AppendLine("-------------------------------------------");
                    rapportBuilder.AppendLine();
                }

                rapportBuilder.AppendLine("EINDE RAPPORT");
                return rapportBuilder.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij genereren rapport: {ex.Message}");
                return "Er is een fout opgetreden bij het genereren van het rapport.";
            }
        }
    }
} 