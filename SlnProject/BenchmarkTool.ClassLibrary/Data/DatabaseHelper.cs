using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace BenchmarkTool.ClassLibrary.Data
{
    /// <summary>
    /// Helper klasse voor database operaties
    /// </summary>
    public class DatabaseHelper
    {
        protected string ConnectionString { get; set; }
        private readonly string _connectionString;

        /// <summary>
        /// Constructor die de connectiestring uit de configuratie ophaalt
        /// </summary>
        public DatabaseHelper()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["BenchmarkDBConnection"].ConnectionString;
            ConnectionString = _connectionString;
        }

        /// <summary>
        /// Constructor met expliciete connectiestring
        /// </summary>
        /// <param name="connectionString">De te gebruiken connectiestring</param>
        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Maakt een nieuwe SqlConnection aan
        /// </summary>
        /// <returns>Een open SqlConnection</returns>
        private SqlConnection GetConnection()
        {
            try
            {
                // Log de connectiestring voor debug doeleinden (verwijder in productie)
                // Format: hide credentials and sensitive information
                string logConnectionString = ConnectionString;
                if (!string.IsNullOrEmpty(logConnectionString))
                {
                    // Vervang credentials met *** voor logging
                    if (logConnectionString.Contains("Password=") || logConnectionString.Contains("pwd="))
                    {
                        // Regex om password te vervangen
                        logConnectionString = System.Text.RegularExpressions.Regex.Replace(
                            logConnectionString,
                            @"(Password|pwd)=([^;]*)(;|$)",
                            "$1=***$3",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Verbinden met database: {logConnectionString}");
                }
                
                var connection = new SqlConnection(ConnectionString);
                
                // Voeg extra robuustheid toe
                connection.InfoMessage += (sender, e) => 
                {
                    System.Diagnostics.Debug.WriteLine($"SQL Info: {e.Message}");
                };
                
                connection.Open();
                return connection;
            }
            catch (SqlException sqlEx)
            {
                // Gedetailleerde foutmelding voor SQL-specifieke fouten
                string errorMessage = $"Database verbindingsfout: {sqlEx.Message}, ErrorCode: {sqlEx.Number}";
                
                // Voeg extra informatie toe voor specifieke foutcodes
                switch(sqlEx.Number)
                {
                    case 4060: // Kan database niet openen
                        errorMessage += ". De opgegeven database bestaat mogelijk niet.";
                        break;
                    case 18456: // Login fout
                        errorMessage += ". Zorg dat de gebruiker toegang heeft tot de database.";
                        break;
                    case 40: // Kan server niet bereiken
                        errorMessage += ". Controleer of de SQL Server actief is en bereikbaar.";
                        break;
                    case 53: // Server niet gevonden
                        errorMessage += ". De opgegeven server kon niet worden gevonden.";
                        break;
                }
                
                System.Diagnostics.Debug.WriteLine(errorMessage);
                System.Diagnostics.Debug.WriteLine($"SQL Stack: {sqlEx.StackTrace}");
                
                throw new Exception("Kan geen verbinding maken met de database. " + errorMessage, sqlEx);
            }
            catch (Exception ex)
            {
                // Algemene fouten
                System.Diagnostics.Debug.WriteLine($"Algemene database fout: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                
                throw new Exception("Kan geen verbinding maken met de database. Controleer de connectiestring en netwerkinstellingen.", ex);
            }
        }

        /// <summary>
        /// Voert een SQL query uit die een DataTable teruggeeft
        /// </summary>
        /// <param name="query">De SQL query</param>
        /// <param name="parameters">De parameters voor de query</param>
        /// <returns>Een DataTable met de resultaten</returns>
        public virtual DataTable ExecuteDataTable(string query, params SqlParameter[] parameters)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Database fout: {ex.Message}");
                throw new Exception("Er is een fout opgetreden bij het uitvoeren van de query.", ex);
            }

            return dataTable;
        }

        /// <summary>
        /// Voert een SQL query uit die een enkele waarde teruggeeft
        /// </summary>
        /// <param name="query">De SQL query</param>
        /// <param name="parameters">De parameters voor de query</param>
        /// <returns>Het resultaat als object</returns>
        public virtual object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Database fout: {ex.Message}");
                throw new Exception("Er is een fout opgetreden bij het uitvoeren van de query.", ex);
            }
        }

        /// <summary>
        /// Voert een SQL query uit die geen resultaten teruggeeft (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="query">De SQL query</param>
        /// <param name="parameters">De parameters voor de query</param>
        /// <returns>Het aantal beïnvloede rijen</returns>
        public virtual int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Database fout: {ex.Message}");
                throw new Exception("Er is een fout opgetreden bij het uitvoeren van de query.", ex);
            }
        }

        /// <summary>
        /// Voert een SQL query uit en retourneert een lijst van objecten van type T
        /// </summary>
        /// <typeparam name="T">Het type object</typeparam>
        /// <param name="query">De SQL query</param>
        /// <param name="map">Functie die een DataRow omzet naar een object van type T</param>
        /// <param name="parameters">De parameters voor de query</param>
        /// <returns>Een lijst van objecten van type T</returns>
        public virtual List<T> ExecuteList<T>(string query, Func<DataRow, T> map, params SqlParameter[] parameters)
        {
            List<T> list = new List<T>();

            try
            {
                DataTable dataTable = ExecuteDataTable(query, parameters);

                foreach (DataRow row in dataTable.Rows)
                {
                    list.Add(map(row));
                }
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Database fout: {ex.Message}");
                throw new Exception("Er is een fout opgetreden bij het uitvoeren van de query.", ex);
            }

            return list;
        }
    }
} 