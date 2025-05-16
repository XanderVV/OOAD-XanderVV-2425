using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace DatabaseTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BenchmarkDB Connectie Test");
            Console.WriteLine("==========================");
            
            string connectionString = "Data Source=localhost\\sqlexpress02;Initial Catalog=BenchmarkDB;Integrated Security=True;Encrypt=False";
            
            Console.WriteLine($"Connectiestring: {connectionString}");
            Console.WriteLine();
            
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Test database verbinding
                    Console.WriteLine("Verbinden met database...");
                    connection.Open();
                    Console.WriteLine("Verbinding succesvol!");
                    Console.WriteLine($"SQL Server versie: {connection.ServerVersion}");
                    Console.WriteLine();
                    
                    // Test Companies tabel
                    TestTabel(connection, "Companies", "SELECT COUNT(*) FROM Companies");
                    
                    // Test overige tabellen
                    TestTabel(connection, "Yearreports", "SELECT COUNT(*) FROM Yearreports");
                    TestTabel(connection, "Categories", "SELECT COUNT(*) FROM Categories");
                    TestTabel(connection, "Questions", "SELECT COUNT(*) FROM Questions");
                    TestTabel(connection, "Answers", "SELECT COUNT(*) FROM Answers");
                    TestTabel(connection, "Costs", "SELECT COUNT(*) FROM Costs");
                    TestTabel(connection, "Costtypes", "SELECT COUNT(*) FROM Costtypes");
                    TestTabel(connection, "Nacecodes", "SELECT COUNT(*) FROM Nacecodes");
                    
                    // Doe een test query op Companies
                    Console.WriteLine("\nCompanies details:");
                    var companiesQuery = "SELECT id, name, login, status FROM Companies";
                    using (SqlCommand command = new SqlCommand(companiesQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                Console.WriteLine("ID\tName\t\tLogin\t\tStatus");
                                Console.WriteLine("------------------------------------------");
                                while (reader.Read())
                                {
                                    int id = reader.GetInt32(0);
                                    string name = reader.IsDBNull(1) ? "NULL" : reader.GetString(1);
                                    string login = reader.IsDBNull(2) ? "NULL" : reader.GetString(2);
                                    string status = reader.IsDBNull(3) ? "NULL" : reader.GetString(3);
                                    
                                    Console.WriteLine($"{id}\t{name}\t{login}\t{status}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Geen Companies data gevonden.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FOUT bij database test:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                }
                Console.WriteLine("\nStackTrace:");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine("\nDruk op een toets om af te sluiten...");
            Console.ReadKey();
        }
        
        static void TestTabel(SqlConnection connection, string tabelNaam, string query)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var result = command.ExecuteScalar();
                    int count = Convert.ToInt32(result);
                    Console.WriteLine($"Tabel {tabelNaam}: {count} rijen gevonden");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FOUT bij testen van tabel {tabelNaam}: {ex.Message}");
            }
        }
    }
} 