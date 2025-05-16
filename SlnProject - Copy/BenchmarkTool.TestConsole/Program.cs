using System;
using System.IO;
using BenchmarkTool.ClassLibrary.CodeQuality;

namespace BenchmarkTool.TestConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BenchmarkTool - Code Kwaliteit Verificatie Tool");
            Console.WriteLine("================================================\n");
            
            string solutionPath = CodeQualityRunner.FindSolutionPath(Directory.GetCurrentDirectory());
            if (string.IsNullOrEmpty(solutionPath))
            {
                Console.WriteLine("Fout: Solution niet gevonden. Voer het programma uit vanuit de solution directory.");
                return;
            }
            
            Console.WriteLine($"Solution gevonden op: {solutionPath}");
            
            string reportsPath = Path.Combine(solutionPath, "CodeQualityReports");
            if (!Directory.Exists(reportsPath))
            {
                Directory.CreateDirectory(reportsPath);
            }
            
            while (true)
            {
                Console.WriteLine("\nKies een optie:");
                Console.WriteLine("1. Volledige codekwaliteitscontrole uitvoeren");
                Console.WriteLine("2. SQL code isolatie controleren");
                Console.WriteLine("3. SQL code kwaliteitsaanbevelingen genereren");
                Console.WriteLine("0. Afsluiten");
                
                Console.Write("\nJouw keuze: ");
                string input = Console.ReadLine();
                
                switch (input)
                {
                    case "1":
                        string fullReportPath = Path.Combine(reportsPath, $"CodeQualityReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                        CodeQualityRunner.RunQualityCheck(solutionPath, fullReportPath);
                        break;
                    
                    case "2":
                        string sqlIsolationPath = Path.Combine(reportsPath, $"SqlIsolationReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                        CodeQualityRunner.RunSqlIsolationCheck(solutionPath, sqlIsolationPath);
                        break;
                    
                    case "3":
                        string sqlRecommendationsPath = Path.Combine(reportsPath, $"SqlRecommendations_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                        CodeQualityRunner.GenerateSqlRecommendations(solutionPath, sqlRecommendationsPath);
                        break;
                    
                    case "0":
                        return;
                    
                    default:
                        Console.WriteLine("Ongeldige keuze, probeer opnieuw.");
                        break;
                }
            }
        }
    }
} 