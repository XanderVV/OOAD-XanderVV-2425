using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using BenchmarkTool.ClassLibrary.Data;
using BenchmarkTool.ClassLibrary.Models;
using BenchmarkTool.ClassLibrary.Services;

namespace BenchmarkTool.Tests
{
    [TestClass]
    public class BenchmarkServiceTests
    {
        private Mock<IBenchmarkRepository> _mockRepository;
        private BenchmarkService _service;

        [TestInitialize]
        public void Initialize()
        {
            _mockRepository = new Mock<IBenchmarkRepository>();
            _service = new BenchmarkService(_mockRepository.Object);
        }

        [TestMethod]
        public void GetBenchmarkData_ValidInput_ReturnsCombinedData()
        {
            // Arrange
            int companyId = 1;
            int year = 2023;
            string naceFilter = "12";
            NaceGroupingLevel groupingLevel = NaceGroupingLevel.Niveau2Cijfers;
            var selectedIndicators = new List<string> { "fte", "cost_salary", "question_1" };

            var benchmarkData = new List<BenchmarkData>
            {
                new BenchmarkData { Indicator = "fte", IndicatorType = "fte", Waarde = 15, Jaar = 2023, NaceCode = "12" },
                new BenchmarkData { Indicator = "cost_salary", IndicatorType = "cost", Waarde = 50000, Jaar = 2023, NaceCode = "12" }
            };

            var ownData = new List<BenchmarkData>
            {
                new BenchmarkData { Indicator = "fte", IndicatorType = "fte", Waarde = 10, Jaar = 2023, NaceCode = "12345" },
                new BenchmarkData { Indicator = "cost_salary", IndicatorType = "cost", Waarde = 45000, Jaar = 2023, NaceCode = "12345" }
            };

            _mockRepository.Setup(r => r.GetBenchmarkData(companyId, year, naceFilter, groupingLevel, selectedIndicators))
                .Returns(benchmarkData);
            
            _mockRepository.Setup(r => r.GetOwnData(companyId, year, selectedIndicators))
                .Returns(ownData);

            // Act
            var result = _service.GetBenchmarkData(companyId, year, naceFilter, groupingLevel, selectedIndicators);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.BenchmarkGegevens.Count);
            Assert.AreEqual(2, result.EigenGegevens.Count);
            
            // Verifieer dat de juiste data is opgehaald
            Assert.AreEqual(15, result.BenchmarkGegevens.First(d => d.Indicator == "fte").Waarde);
            Assert.AreEqual(50000, result.BenchmarkGegevens.First(d => d.Indicator == "cost_salary").Waarde);
            Assert.AreEqual(10, result.EigenGegevens.First(d => d.Indicator == "fte").Waarde);
            Assert.AreEqual(45000, result.EigenGegevens.First(d => d.Indicator == "cost_salary").Waarde);
        }

        [TestMethod]
        public void BerekenStatistieken_ValidInput_ReturnsCorrectStatistics()
        {
            // Arrange
            var benchmarkData = new List<BenchmarkData>
            {
                new BenchmarkData { Indicator = "fte", Waarde = 10 },
                new BenchmarkData { Indicator = "fte", Waarde = 20 },
                new BenchmarkData { Indicator = "fte", Waarde = 30 },
                new BenchmarkData { Indicator = "fte", Waarde = 40 },
                new BenchmarkData { Indicator = "fte", Waarde = 50 }
            };

            // Act
            var result = _service.BerekenStatistieken(benchmarkData, "fte");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey("Gemiddelde"));
            Assert.IsTrue(result.ContainsKey("Mediaan"));
            Assert.IsTrue(result.ContainsKey("Minimum"));
            Assert.IsTrue(result.ContainsKey("Maximum"));
            Assert.IsTrue(result.ContainsKey("Standaardafwijking"));
            Assert.IsTrue(result.ContainsKey("Percentiel25"));
            Assert.IsTrue(result.ContainsKey("Percentiel75"));
            
            // Verifieer berekeningen
            Assert.AreEqual(30, result["Gemiddelde"]);
            Assert.AreEqual(30, result["Mediaan"]);
            Assert.AreEqual(10, result["Minimum"]);
            Assert.AreEqual(50, result["Maximum"]);
            Assert.AreEqual(20, result["Percentiel25"]);
            Assert.AreEqual(40, result["Percentiel75"]);
            Assert.AreEqual(5, result["AantalWaarnemingen"]);
        }

        [TestMethod]
        public void GenereerRapport_WithBenchmarkData_ReturnsFormattedReport()
        {
            // Arrange
            var benchmarkResultaat = new BenchmarkResultaat
            {
                BenchmarkGegevens = new List<BenchmarkData>
                {
                    new BenchmarkData { Indicator = "fte", IndicatorType = "fte", Waarde = 20 },
                    new BenchmarkData { Indicator = "fte", IndicatorType = "fte", Waarde = 30 },
                    new BenchmarkData { Indicator = "cost_salary", IndicatorType = "cost", Waarde = 50000 },
                    new BenchmarkData { Indicator = "cost_salary", IndicatorType = "cost", Waarde = 60000 }
                },
                EigenGegevens = new List<BenchmarkData>
                {
                    new BenchmarkData { Indicator = "fte", IndicatorType = "fte", Waarde = 30 },
                    new BenchmarkData { Indicator = "cost_salary", IndicatorType = "cost", Waarde = 40000 }
                }
            };

            // Act
            var result = _service.GenereerRapport(benchmarkResultaat);
            
            // Debug output
            Console.WriteLine("Begin rapport output:");
            Console.WriteLine(result);
            Console.WriteLine("Einde rapport output");
            
            Console.WriteLine($"Bevat 'BENCHMARK RAPPORT': {result.Contains("BENCHMARK RAPPORT")}");
            Console.WriteLine($"Bevat 'INDICATOR: fte': {result.Contains("INDICATOR: fte")}");
            Console.WriteLine($"Bevat 'INDICATOR: Kosten - salary': {result.Contains("INDICATOR: Kosten - salary")}");
            Console.WriteLine($"Bevat 'Eigen waarde: 30,00': {result.Contains("Eigen waarde: 30,00")}");
            Console.WriteLine($"Bevat 'Eigen waarde: 40.000,00': {result.Contains("Eigen waarde: 40.000,00")}");
            Console.WriteLine($"Bevat 'STERK PUNT': {result.Contains("STERK PUNT")}");
            Console.WriteLine($"Bevat 'AANDACHTSPUNT': {result.Contains("AANDACHTSPUNT")}");
            Console.WriteLine($"Bevat 'ligt dicht bij het benchmark gemiddelde': {result.Contains("ligt dicht bij het benchmark gemiddelde")}");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("BENCHMARK RAPPORT"));
            Assert.IsTrue(result.Contains("INDICATOR: fte"));
            Assert.IsTrue(result.Contains("INDICATOR: Kosten - salary"));
            Assert.IsTrue(result.Contains("Eigen waarde: 30,00"));
            Assert.IsTrue(result.Contains("Eigen waarde: 40.000,00"));
            
            // Check for specific analysis
            Assert.IsTrue(result.Contains("STERK PUNT") || result.Contains("AANDACHTSPUNT") || 
                          result.Contains("ligt dicht bij het benchmark gemiddelde"));
        }
    }
} 