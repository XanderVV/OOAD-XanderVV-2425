using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using BenchmarkTool.ClassLibrary.Models;

namespace BenchmarkTool.ClassLibrary.Data
{
    /// <summary>
    /// Repository klasse voor database operaties specifiek voor de BenchmarkTool
    /// </summary>
    public class DatabaseRepository
    {
        protected readonly DatabaseHelper _dbHelper;

        /// <summary>
        /// Constructor met standaard DatabaseHelper
        /// </summary>
        public DatabaseRepository()
        {
            _dbHelper = new DatabaseHelper();
        }

        /// <summary>
        /// Constructor met expliciete DatabaseHelper
        /// </summary>
        /// <param name="dbHelper">De te gebruiken DatabaseHelper</param>
        public DatabaseRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        #region Generic Methods

        /// <summary>
        /// Haalt een enkele rij op uit de database op basis van ID
        /// </summary>
        /// <typeparam name="T">Type van het object dat moet worden opgehaald</typeparam>
        /// <param name="tableName">Naam van de tabel</param>
        /// <param name="idColumnName">Naam van de ID-kolom</param>
        /// <param name="id">ID-waarde</param>
        /// <param name="map">Mapping functie om een DataRow naar T te converteren</param>
        /// <returns>Het opgehaalde object of null als geen rij werd gevonden</returns>
        protected T GetById<T>(string tableName, string idColumnName, object id, Func<DataRow, T> map) where T : class
        {
            string query = $"SELECT * FROM {tableName} WHERE {idColumnName} = @Id";
            SqlParameter parameter = new SqlParameter("@Id", id);

            DataTable result = _dbHelper.ExecuteDataTable(query, parameter);

            if (result.Rows.Count == 0)
            {
                return null;
            }

            return map(result.Rows[0]);
        }

        /// <summary>
        /// Haalt alle rijen op uit een tabel
        /// </summary>
        /// <typeparam name="T">Type van de objecten die moeten worden opgehaald</typeparam>
        /// <param name="tableName">Naam van de tabel</param>
        /// <param name="map">Mapping functie om een DataRow naar T te converteren</param>
        /// <returns>Lijst van opgehaalde objecten</returns>
        protected List<T> GetAll<T>(string tableName, Func<DataRow, T> map)
        {
            string query = $"SELECT * FROM {tableName}";
            return _dbHelper.ExecuteList(query, map);
        }

        /// <summary>
        /// Verwijdert een rij uit de database op basis van ID
        /// </summary>
        /// <param name="tableName">Naam van de tabel</param>
        /// <param name="idColumnName">Naam van de ID-kolom</param>
        /// <param name="id">ID-waarde</param>
        /// <returns>True als de rij succesvol is verwijderd</returns>
        protected bool DeleteById(string tableName, string idColumnName, object id)
        {
            string query = $"DELETE FROM {tableName} WHERE {idColumnName} = @Id";
            SqlParameter parameter = new SqlParameter("@Id", id);

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameter);
            return rowsAffected > 0;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Converteert een nullable database waarde naar het opgegeven type
        /// </summary>
        /// <typeparam name="T">Doel-type</typeparam>
        /// <param name="value">De database waarde</param>
        /// <returns>De geconverteerde waarde of default(T) als de database waarde null is</returns>
        protected T ConvertFromDBValue<T>(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return default;
            }

            Type targetType = typeof(T);
            
            // Controleer of het doeltype een nullable value type is (zoals int?, decimal?, etc.)
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Haal het onderliggende type op (bijv. int voor int?)
                Type underlyingType = Nullable.GetUnderlyingType(targetType);
                
                // Converteer naar het onderliggende type en wrap in een nullable
                var convertedValue = Convert.ChangeType(value, underlyingType);
                
                // Creëer een nullable instantie met de waarde
                return (T)convertedValue;
            }
            
            // Normale conversie voor niet-nullable types
            return (T)Convert.ChangeType(value, targetType);
        }

        /// <summary>
        /// Converteert een waarde naar een database parameter
        /// </summary>
        /// <param name="value">De waarde</param>
        /// <returns>De waarde of DBNull.Value als de waarde null is</returns>
        protected object ConvertToDBValue(object value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            return value;
        }

        #endregion

        #region Stamgegevens Ophalen

        /// <summary>
        /// Haalt alle NACE-codes op uit de database
        /// </summary>
        /// <returns>Lijst met alle NACE-codes</returns>
        public virtual List<Nacecode> GetAllNacecodes()
        {
            return GetAll("Nacecodes", row => new Nacecode
            {
                Code = ConvertFromDBValue<string>(row["code"]),
                Text = ConvertFromDBValue<string>(row["text"]),
                TextFr = ConvertFromDBValue<string>(row["textFr"]),
                TextEn = ConvertFromDBValue<string>(row["textEn"]),
                ParentCode = ConvertFromDBValue<string>(row["parent_code"])
            });
        }

        /// <summary>
        /// Haalt alle categorieën op uit de database
        /// </summary>
        /// <returns>Lijst met alle categorieën</returns>
        public virtual List<Categorie> GetAllCategories()
        {
            return GetAll("Categories", row => new Categorie
            {
                Nr = ConvertFromDBValue<int>(row["nr"]),
                Text = ConvertFromDBValue<string>(row["text"]),
                TextFr = ConvertFromDBValue<string>(row["textFr"]),
                TextEn = ConvertFromDBValue<string>(row["textEn"]),
                Tooltip = ConvertFromDBValue<string>(row["tooltip"]),
                TooltipFr = ConvertFromDBValue<string>(row["tooltipFr"]),
                TooltipEn = ConvertFromDBValue<string>(row["tooltipEn"]),
                RelevantCostTypes = ConvertFromDBValue<string>(row["relevantCostTypes"]),
                ParentNr = ConvertFromDBValue<int?>(row["parent_nr"])
            });
        }

        /// <summary>
        /// Haalt alle vragen op uit de database
        /// </summary>
        /// <returns>Lijst met alle vragen</returns>
        public virtual List<Vraag> GetAllQuestions()
        {
            return GetAll("Questions", row => new Vraag
            {
                Id = ConvertFromDBValue<int>(row["id"]),
                Text = ConvertFromDBValue<string>(row["text"]),
                TextFr = ConvertFromDBValue<string>(row["textFr"]),
                TextEn = ConvertFromDBValue<string>(row["textEn"]),
                Tooltip = ConvertFromDBValue<string>(row["tooltip"]),
                TooltipFr = ConvertFromDBValue<string>(row["tooltipFr"]),
                TooltipEn = ConvertFromDBValue<string>(row["tooltipEn"]),
                Type = ConvertFromDBValue<string>(row["type"]),
                Values = ConvertFromDBValue<string>(row["values"]),
                ValuesFr = ConvertFromDBValue<string>(row["valuesFr"]),
                ValuesEn = ConvertFromDBValue<string>(row["valuesEn"]),
                MaxValue = ConvertFromDBValue<decimal?>(row["maxvalue"]),
                CategoryNr = ConvertFromDBValue<int>(row["category_nr"])
            });
        }

        /// <summary>
        /// Haalt alle kosttypes op uit de database
        /// </summary>
        /// <returns>Lijst met alle kosttypes</returns>
        public virtual List<KostType> GetAllCosttypes()
        {
            return GetAll("Costtypes", row => new KostType
            {
                Type = ConvertFromDBValue<string>(row["type"]),
                Text = ConvertFromDBValue<string>(row["text"]),
                TextFr = ConvertFromDBValue<string>(row["textFr"]),
                TextEn = ConvertFromDBValue<string>(row["textEn"])
            });
        }

        #endregion
    }
} 