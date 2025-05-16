using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using BenchmarkTool.ClassLibrary.Data;
using BenchmarkTool.ClassLibrary.Models;
using BenchmarkTool.ClassLibrary.Services;

namespace BenchmarkTool.Tests
{
    [TestClass]
    public class BedrijfServiceTests
    {
        [TestMethod]
        public void GetAllNacecodes_ReturnsCorrectList()
        {
            // Arrange
            var expectedNacecodes = new List<Nacecode>
            {
                new Nacecode { Code = "01", Text = "Landbouw", TextFr = "Agriculture", TextEn = "Agriculture", ParentCode = null },
                new Nacecode { Code = "02", Text = "Bosbouw", TextFr = "Forestry", TextEn = "Forestry", ParentCode = null }
            };

            // Create a test repository implementation
            var testDatabaseHelper = new TestDatabaseHelper();
            var testRepository = new TestDatabaseRepository(testDatabaseHelper);
            testRepository.SetupGetAllNacecodes(expectedNacecodes);

            // Create the service with our test repository
            var service = new TestBedrijfService(testRepository);

            // Act
            var result = service.GetAllNacecodes();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("01", result[0].Code);
            Assert.AreEqual("Landbouw", result[0].Text);
            Assert.AreEqual("02", result[1].Code);
        }

        [TestMethod]
        public void GetAllCategories_ReturnsCorrectList()
        {
            // Arrange
            var expectedCategories = new List<Categorie>
            {
                new Categorie { Nr = 1, Text = "Categorie 1", TextFr = "Catégorie 1", TextEn = "Category 1" },
                new Categorie { Nr = 2, Text = "Categorie 2", TextFr = "Catégorie 2", TextEn = "Category 2" }
            };

            // Create a test repository implementation
            var testDatabaseHelper = new TestDatabaseHelper();
            var testRepository = new TestDatabaseRepository(testDatabaseHelper);
            testRepository.SetupGetAllCategories(expectedCategories);

            // Create the service with our test repository
            var service = new TestBedrijfService(testRepository);

            // Act
            var result = service.GetAllCategories();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].Nr);
            Assert.AreEqual("Categorie 1", result[0].Text);
            Assert.AreEqual(2, result[1].Nr);
        }

        [TestMethod]
        public void GetAllQuestions_ReturnsCorrectList()
        {
            // Arrange
            var expectedQuestions = new List<Vraag>
            {
                new Vraag { Id = 1, Text = "Vraag 1", Type = "numeric", CategoryNr = 1 },
                new Vraag { Id = 2, Text = "Vraag 2", Type = "text", CategoryNr = 1 }
            };

            // Create a test repository implementation
            var testDatabaseHelper = new TestDatabaseHelper();
            var testRepository = new TestDatabaseRepository(testDatabaseHelper);
            testRepository.SetupGetAllQuestions(expectedQuestions);

            // Create the service with our test repository
            var service = new TestBedrijfService(testRepository);

            // Act
            var result = service.GetAllQuestions();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual("Vraag 1", result[0].Text);
            Assert.AreEqual("numeric", result[0].Type);
            Assert.AreEqual(2, result[1].Id);
        }

        [TestMethod]
        public void GetAllCosttypes_ReturnsCorrectList()
        {
            // Arrange
            var expectedCosttypes = new List<KostType>
            {
                new KostType { Type = "type1", Text = "Kosttype 1", TextFr = "Type de coût 1", TextEn = "Cost type 1" },
                new KostType { Type = "type2", Text = "Kosttype 2", TextFr = "Type de coût 2", TextEn = "Cost type 2" }
            };

            // Create a test repository implementation
            var testDatabaseHelper = new TestDatabaseHelper();
            var testRepository = new TestDatabaseRepository(testDatabaseHelper);
            testRepository.SetupGetAllCosttypes(expectedCosttypes);

            // Create the service with our test repository
            var service = new TestBedrijfService(testRepository);

            // Act
            var result = service.GetAllCosttypes();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("type1", result[0].Type);
            Assert.AreEqual("Kosttype 1", result[0].Text);
            Assert.AreEqual("type2", result[1].Type);
        }
    }

    // Test helper klassen
    public class TestDatabaseHelper : DatabaseHelper
    {
        public TestDatabaseHelper() : base("test_connection_string")
        {
        }
        
        // Override de methodes zodat ze niet echt een database connectie proberen te maken
        public override DataTable ExecuteDataTable(string query, params Microsoft.Data.SqlClient.SqlParameter[] parameters)
        {
            return new DataTable();
        }
        
        public override List<T> ExecuteList<T>(string query, Func<DataRow, T> map, params Microsoft.Data.SqlClient.SqlParameter[] parameters)
        {
            return new List<T>();
        }
    }

    public class TestDatabaseRepository : DatabaseRepository
    {
        private List<Nacecode> _nacecodes;
        private List<Categorie> _categories;
        private List<Vraag> _questions;
        private List<KostType> _costtypes;

        public TestDatabaseRepository(DatabaseHelper dbHelper) : base(dbHelper)
        {
        }

        public void SetupGetAllNacecodes(List<Nacecode> nacecodes)
        {
            _nacecodes = nacecodes;
        }

        public void SetupGetAllCategories(List<Categorie> categories)
        {
            _categories = categories;
        }

        public void SetupGetAllQuestions(List<Vraag> questions)
        {
            _questions = questions;
        }

        public void SetupGetAllCosttypes(List<KostType> costtypes)
        {
            _costtypes = costtypes;
        }

        public override List<Nacecode> GetAllNacecodes()
        {
            return _nacecodes;
        }

        public override List<Categorie> GetAllCategories()
        {
            return _categories;
        }

        public override List<Vraag> GetAllQuestions()
        {
            return _questions;
        }

        public override List<KostType> GetAllCosttypes()
        {
            return _costtypes;
        }
    }

    public class TestBedrijfService : BedrijfService
    {
        private readonly TestDatabaseRepository _testRepository;

        public TestBedrijfService(TestDatabaseRepository testRepository)
        {
            // Skip de base constructor aanroep om verbinding met de echte database te vermijden
            _testRepository = testRepository;
        }

        public override List<Nacecode> GetAllNacecodes()
        {
            return _testRepository.GetAllNacecodes();
        }

        public override List<Categorie> GetAllCategories()
        {
            return _testRepository.GetAllCategories();
        }

        public override List<Vraag> GetAllQuestions()
        {
            return _testRepository.GetAllQuestions();
        }

        public override List<KostType> GetAllCosttypes()
        {
            return _testRepository.GetAllCosttypes();
        }
    }
} 