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
        private readonly string _connectionString;

        /// <summary>
        /// Constructor die de connectiestring uit de configuratie ophaalt
        /// </summary>
        public DatabaseHelper()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["BenchmarkDBConnection"].ConnectionString;
        }

        /// <summary>
        /// Constructor met expliciete connectiestring
        /// </summary>
        /// <param name="connectionString">De te gebruiken connectiestring</param>
        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Maakt een nieuwe SqlConnection aan
        /// </summary>
        /// <returns>Een open SqlConnection</returns>
        private SqlConnection GetConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
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
        public object ExecuteScalar(string query, params SqlParameter[] parameters)
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
        public int ExecuteNonQuery(string query, params SqlParameter[] parameters)
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