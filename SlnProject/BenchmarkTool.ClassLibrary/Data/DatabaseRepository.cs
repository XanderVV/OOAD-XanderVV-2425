using System;
using System.Collections.Generic;
using BenchmarkTool.ClassLibrary.Models;
using Microsoft.Data.SqlClient;

namespace BenchmarkTool.ClassLibrary.Data
{
    /// <summary>
    /// Repository klasse voor database operaties specifiek voor de BenchmarkTool
    /// </summary>
    public class DatabaseRepository
    {
        private readonly DatabaseHelper _dbHelper;

        /// <summary>
        /// Beschermde eigenschap om toegang te krijgen tot de database helper
        /// </summary>
        protected DatabaseHelper DbHelper => _dbHelper;

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
        /// <param name="map">Mapping functie om een SqlDataReader naar T te converteren</param>
        /// <returns>Het opgehaalde object of null als geen rij werd gevonden</returns>
        protected T GetById<T>(string tableName, string idColumnName, object id, Func<SqlDataReader, T> map) 
            where T : class
        {
            string query = $"SELECT * FROM {tableName} WHERE {idColumnName} = @Id";
            SqlParameter parameter = new SqlParameter("@Id", id);

            T result = default;
            DbHelper.ExecuteReader(
                query, 
                delegate(SqlDataReader reader) 
                {
                    if (reader.Read())
                    {
                        result = map(reader);
                    }
                }, 
                parameter);

            return result;
        }

        /// <summary>
        /// Haalt alle rijen op uit een tabel
        /// </summary>
        /// <typeparam name="T">Type van de objecten die moeten worden opgehaald</typeparam>
        /// <param name="tableName">Naam van de tabel</param>
        /// <param name="map">Mapping functie om een SqlDataReader naar T te converteren</param>
        /// <returns>Lijst van opgehaalde objecten</returns>
        protected List<T> GetAll<T>(string tableName, Func<SqlDataReader, T> map)
        {
            string query = $"SELECT * FROM {tableName}";
            return DbHelper.ExecuteList(query, map);
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

            int rowsAffected = DbHelper.ExecuteNonQuery(query, parameter);
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
            return GetAll("Nacecodes", reader => new Nacecode
            {
                Code = ConvertFromDBValue<string>(reader["code"]),
                Text = ConvertFromDBValue<string>(reader["text"]),
                TextFr = ConvertFromDBValue<string>(reader["textFr"]),
                TextEn = ConvertFromDBValue<string>(reader["textEn"]),
                ParentCode = ConvertFromDBValue<string>(reader["parent_code"])
            });
        }

        /// <summary>
        /// Haalt alle categorieën op uit de database
        /// </summary>
        /// <returns>Lijst met alle categorieën</returns>
        public virtual List<Categorie> GetAllCategories()
        {
            return GetAll("Categories", reader => new Categorie
            {
                Nr = ConvertFromDBValue<int>(reader["nr"]),
                Text = ConvertFromDBValue<string>(reader["text"]),
                TextFr = ConvertFromDBValue<string>(reader["textFr"]),
                TextEn = ConvertFromDBValue<string>(reader["textEn"]),
                Tooltip = ConvertFromDBValue<string>(reader["tooltip"]),
                TooltipFr = ConvertFromDBValue<string>(reader["tooltipFr"]),
                TooltipEn = ConvertFromDBValue<string>(reader["tooltipEn"]),
                RelevantCostTypes = ConvertFromDBValue<string>(reader["relevantCostTypes"]),
                ParentNr = ConvertFromDBValue<int?>(reader["parent_nr"])
            });
        }

        /// <summary>
        /// Haalt alle vragen op uit de database
        /// </summary>
        /// <returns>Lijst met alle vragen</returns>
        public virtual List<Vraag> GetAllQuestions()
        {
            return GetAll("Questions", reader => new Vraag
            {
                Id = ConvertFromDBValue<int>(reader["id"]),
                Text = ConvertFromDBValue<string>(reader["text"]),
                TextFr = ConvertFromDBValue<string>(reader["textFr"]),
                TextEn = ConvertFromDBValue<string>(reader["textEn"]),
                Tooltip = ConvertFromDBValue<string>(reader["tooltip"]),
                TooltipFr = ConvertFromDBValue<string>(reader["tooltipFr"]),
                TooltipEn = ConvertFromDBValue<string>(reader["tooltipEn"]),
                Type = ConvertFromDBValue<string>(reader["type"]),
                Values = ConvertFromDBValue<string>(reader["values"]),
                ValuesFr = ConvertFromDBValue<string>(reader["valuesFr"]),
                ValuesEn = ConvertFromDBValue<string>(reader["valuesEn"]),
                MaxValue = ConvertFromDBValue<decimal?>(reader["maxvalue"]),
                CategoryNr = ConvertFromDBValue<int>(reader["category_nr"])
            });
        }

        /// <summary>
        /// Haalt alle kosttypes op uit de database
        /// </summary>
        /// <returns>Lijst met alle kosttypes</returns>
        public virtual List<KostType> GetAllCosttypes()
        {
            return GetAll("Costtypes", reader => new KostType
            {
                Type = ConvertFromDBValue<string>(reader["type"]),
                Text = ConvertFromDBValue<string>(reader["text"]),
                TextFr = ConvertFromDBValue<string>(reader["textFr"]),
                TextEn = ConvertFromDBValue<string>(reader["textEn"])
            });
        }

        #endregion
    }
} 