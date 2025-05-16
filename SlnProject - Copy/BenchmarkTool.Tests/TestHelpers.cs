using System;
using System.Collections.Generic;
using System.Data;
using BenchmarkTool.ClassLibrary.Data;
using BenchmarkTool.ClassLibrary.Models;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BenchmarkTool.Tests
{
    [TestClass]
    public class StamgegevensTests
    {
        // Controleert de implementatie van de stamgegevens methodes in DatabaseRepository
        
        [TestMethod]
        public void GetAllNacecodes_ReturnsNonNullList()
        {
            // Arrange
            var testDatabaseHelper = new TestDatabaseHelper();
            var repository = new DatabaseRepository(testDatabaseHelper);

            // Act
            var result = repository.GetAllNacecodes();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<Nacecode>));
        }

        [TestMethod]
        public void GetAllCategories_ReturnsNonNullList()
        {
            // Arrange
            var testDatabaseHelper = new TestDatabaseHelper();
            var repository = new DatabaseRepository(testDatabaseHelper);

            // Act
            var result = repository.GetAllCategories();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<Categorie>));
        }

        [TestMethod]
        public void GetAllQuestions_ReturnsNonNullList()
        {
            // Arrange
            var testDatabaseHelper = new TestDatabaseHelper();
            var repository = new DatabaseRepository(testDatabaseHelper);

            // Act
            var result = repository.GetAllQuestions();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<Vraag>));
        }

        [TestMethod]
        public void GetAllCosttypes_ReturnsNonNullList()
        {
            // Arrange
            var testDatabaseHelper = new TestDatabaseHelper();
            var repository = new DatabaseRepository(testDatabaseHelper);

            // Act
            var result = repository.GetAllCosttypes();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<KostType>));
        }
    }
} 