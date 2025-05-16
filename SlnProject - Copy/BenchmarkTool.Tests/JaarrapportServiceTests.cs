using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkTool.ClassLibrary.Data;
using BenchmarkTool.ClassLibrary.Models;
using BenchmarkTool.ClassLibrary.Services;

namespace BenchmarkTool.Tests
{
    [TestClass]
    public class JaarrapportServiceTests
    {
        private Mock<IJaarrapportRepository> _mockRepository;
        private JaarrapportService _service;

        [TestInitialize]
        public void Initialize()
        {
            _mockRepository = new Mock<IJaarrapportRepository>();
            _service = new JaarrapportService(_mockRepository.Object);
        }

        [TestMethod]
        public void CreateYearreport_WhenValidInput_ReturnsNewId()
        {
            // Arrange
            var jaarrapport = new Jaarrapport
            {
                Year = 2023,
                Fte = 10,
                CompanyId = 1
            };

            _mockRepository.Setup(r => r.GetByYearAndCompany(jaarrapport.Year, jaarrapport.CompanyId))
                .Returns((Jaarrapport)null);
            
            _mockRepository.Setup(r => r.Create(jaarrapport))
                .Returns(42);

            // Act
            var result = _service.CreateYearreport(jaarrapport);

            // Assert
            Assert.AreEqual(42, result);
            _mockRepository.Verify(r => r.Create(jaarrapport), Times.Once);
        }

        [TestMethod]
        public void CreateYearreport_WhenDuplicateYearAndCompany_ReturnsMinusOne()
        {
            // Arrange
            var jaarrapport = new Jaarrapport
            {
                Year = 2023,
                Fte = 10,
                CompanyId = 1
            };

            var existingJaarrapport = new Jaarrapport
            {
                Id = 42,
                Year = 2023,
                Fte = 5,
                CompanyId = 1
            };

            _mockRepository.Setup(r => r.GetByYearAndCompany(jaarrapport.Year, jaarrapport.CompanyId))
                .Returns(existingJaarrapport);

            // Act
            var result = _service.CreateYearreport(jaarrapport);

            // Assert
            Assert.AreEqual(-1, result);
            _mockRepository.Verify(r => r.Create(jaarrapport), Times.Never);
        }

        [TestMethod]
        public void GetYearreportsByCompany_WhenValidCompanyId_ReturnsReports()
        {
            // Arrange
            int companyId = 1;
            var expected = new List<Jaarrapport>
            {
                new Jaarrapport { Id = 1, Year = 2023, Fte = 10, CompanyId = companyId },
                new Jaarrapport { Id = 2, Year = 2022, Fte = 9, CompanyId = companyId }
            };

            _mockRepository.Setup(r => r.GetByCompanyId(companyId))
                .Returns(expected);

            // Act
            var result = _service.GetYearreportsByCompany(companyId);

            // Assert
            CollectionAssert.AreEqual(expected, result.ToList());
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetYearreportDetails_WhenValidReportId_ReturnsDetails()
        {
            // Arrange
            int reportId = 1;
            var jaarrapport = new Jaarrapport { Id = reportId, Year = 2023, Fte = 10, CompanyId = 1 };
            var kosten = new List<Kost>
            {
                new Kost { Id = 1, Value = 1000, CosttypeType = "Type1", CategoryNr = 1, YearreportId = reportId }
            };
            var antwoorden = new List<Antwoord>
            {
                new Antwoord { Id = 1, Value = "Antwoord1", QuestionId = 1, YearreportId = reportId }
            };

            _mockRepository.Setup(r => r.GetById(reportId)).Returns(jaarrapport);
            _mockRepository.Setup(r => r.GetCosts(reportId)).Returns(kosten);
            _mockRepository.Setup(r => r.GetAnswers(reportId)).Returns(antwoorden);

            // Act
            var result = _service.GetYearreportDetails(reportId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(jaarrapport, result["Jaarrapport"]);
            CollectionAssert.AreEqual(kosten, ((List<Kost>)result["Kosten"]).ToList());
            CollectionAssert.AreEqual(antwoorden, ((List<Antwoord>)result["Antwoorden"]).ToList());
        }

        [TestMethod]
        public void UpdateYearreport_WhenValidInput_ReturnsTrue()
        {
            // Arrange
            var jaarrapport = new Jaarrapport
            {
                Id = 1,
                Year = 2023,
                Fte = 10,
                CompanyId = 1
            };

            _mockRepository.Setup(r => r.GetById(jaarrapport.Id))
                .Returns(jaarrapport);
            
            _mockRepository.Setup(r => r.Update(jaarrapport))
                .Returns(true);

            // Act
            var result = _service.UpdateYearreport(jaarrapport);

            // Assert
            Assert.IsTrue(result);
            _mockRepository.Verify(r => r.Update(jaarrapport), Times.Once);
        }

        [TestMethod]
        public void DeleteYearreport_WhenValidReportId_ReturnsTrue()
        {
            // Arrange
            int reportId = 1;
            var jaarrapport = new Jaarrapport { Id = reportId, Year = 2023, Fte = 10, CompanyId = 1 };

            _mockRepository.Setup(r => r.GetById(reportId))
                .Returns(jaarrapport);
            
            _mockRepository.Setup(r => r.Delete(reportId))
                .Returns(true);

            // Act
            var result = _service.DeleteYearreport(reportId);

            // Assert
            Assert.IsTrue(result);
            _mockRepository.Verify(r => r.Delete(reportId), Times.Once);
        }

        [TestMethod]
        public void SaveCostsForReport_WhenValidInput_ReturnsCount()
        {
            // Arrange
            int reportId = 1;
            var jaarrapport = new Jaarrapport { Id = reportId, Year = 2023, Fte = 10, CompanyId = 1 };
            var kosten = new List<Kost>
            {
                new Kost { Value = 1000, CosttypeType = "Type1", CategoryNr = 1 },
                new Kost { Value = 2000, CosttypeType = "Type2", CategoryNr = 2 }
            };

            _mockRepository.Setup(r => r.GetById(reportId))
                .Returns(jaarrapport);
            
            _mockRepository.Setup(r => r.SaveCosts(reportId, It.IsAny<List<Kost>>()))
                .Returns(kosten.Count);

            // Act
            var result = _service.SaveCostsForReport(reportId, kosten);

            // Assert
            Assert.AreEqual(kosten.Count, result);
            
            // Verifieer dat alle kosten het juiste jaarrapport ID hebben
            foreach (var kost in kosten)
            {
                Assert.AreEqual(reportId, kost.YearreportId);
            }
            
            _mockRepository.Verify(r => r.SaveCosts(reportId, kosten), Times.Once);
        }

        [TestMethod]
        public void SaveAnswersForReport_WhenValidInput_FiltersInfoTypeQuestions()
        {
            // Arrange
            int reportId = 1;
            var jaarrapport = new Jaarrapport { Id = reportId, Year = 2023, Fte = 10, CompanyId = 1 };
            
            var relevantVragen = new List<Vraag>
            {
                new Vraag { Id = 1, Type = "numeric" },
                new Vraag { Id = 3, Type = "text" }
            };
            
            var antwoorden = new List<Antwoord>
            {
                new Antwoord { QuestionId = 1, Value = "Antwoord1" }, // Moet behouden blijven
                new Antwoord { QuestionId = 2, Value = "Antwoord2" }, // Moet gefilterd worden
                new Antwoord { QuestionId = 3, Value = "Antwoord3" }  // Moet behouden blijven
            };

            _mockRepository.Setup(r => r.GetById(reportId))
                .Returns(jaarrapport);
            
            _mockRepository.Setup(r => r.GetQuestionsNotInfo())
                .Returns(relevantVragen);
            
            _mockRepository.Setup(r => r.SaveAnswers(reportId, It.IsAny<List<Antwoord>>()))
                .Returns((int reportId, List<Antwoord> answers) => answers.Count);

            // Act
            var result = _service.SaveAnswersForReport(reportId, antwoorden);

            // Assert
            // Twee antwoorden zouden bewaard moeten zijn
            Assert.AreEqual(2, result);
            
            // Verifieer dat het filter correct is toegepast
            _mockRepository.Verify(r => r.SaveAnswers(
                reportId, 
                It.Is<List<Antwoord>>(list => 
                    list.Count == 2 && 
                    list.All(a => a.QuestionId == 1 || a.QuestionId == 3)
                )), 
                Times.Once);
        }
    }
} 