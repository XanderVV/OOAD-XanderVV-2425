using System;
using System.IO;

namespace BenchmarkTool.ClassLibrary.CodeQuality
{
    /// <summary>
    /// Hulpprogramma voor het uitvoeren van codekwaliteitscontroles
    /// </summary>
    public static class CodeQualityRunner
    {
        /// <summary>
        /// Voert een volledige kwaliteitscontrole uit op het project en schrijft het rapport naar een bestand
        /// </summary>
        /// <param name="solutionPath">Pad naar de solution</param>
        /// <param name="outputFilePath">Pad waar het rapport geschreven moet worden</param>
        /// <returns>True als de controle succesvol is uitgevoerd, anders false</returns>
        public static bool RunQualityCheck(string solutionPath, string outputFilePath)
        {
            try
            {
                Console.WriteLine("Start codekwaliteitscontrole...");
                
                var verifier = new CodeQualityVerifier(solutionPath);
                var report = verifier.VerifyCodeQuality();
                
                File.WriteAllText(outputFilePath, report.ToString());
                
                Console.WriteLine($"Codekwaliteitsrapport is geschreven naar: {outputFilePath}");
                
                if (report.HasIssues)
                {
                    Console.WriteLine("Waarschuwing: Er zijn codekwaliteitsproblemen gevonden.");
                    Console.WriteLine("Bekijk het rapport voor details en aanbevelingen.");
                    return false;
                }
                
                Console.WriteLine("Codekwaliteitscontrole succesvol afgerond zonder problemen.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij het uitvoeren van codekwaliteitscontrole: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Voert alleen de SQL code isolatie controle uit
        /// </summary>
        /// <param name="solutionPath">Pad naar de solution</param>
        /// <param name="outputFilePath">Pad waar het rapport geschreven moet worden</param>
        /// <returns>True als de controle succesvol is uitgevoerd en geen schendingen zijn gevonden, anders false</returns>
        public static bool RunSqlIsolationCheck(string solutionPath, string outputFilePath)
        {
            try
            {
                Console.WriteLine("Start SQL code isolatie controle...");
                
                var sqlVerifier = new SqlCodeVerifier(solutionPath);
                var violations = sqlVerifier.VerifySqlCodeIsolation();
                
                if (violations.Count > 0)
                {
                    string report = $"SQL Code Isolatie Rapport ({DateTime.Now}):\n\n";
                    report += $"Gevonden schendingen: {violations.Count}\n\n";
                    
                    foreach (var violation in violations)
                    {
                        report += $"- {violation}\n";
                    }
                    
                    File.WriteAllText(outputFilePath, report);
                    
                    Console.WriteLine($"SQL isolatierapport is geschreven naar: {outputFilePath}");
                    Console.WriteLine($"Waarschuwing: Er zijn {violations.Count} schendingen van SQL code isolatie gevonden.");
                    return false;
                }
                
                File.WriteAllText(outputFilePath, "SQL Code Isolatie Rapport: Geen schendingen gevonden.");
                Console.WriteLine("SQL isolatiecontrole succesvol afgerond zonder problemen.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij het uitvoeren van SQL isolatiecontrole: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Genereert aanbevelingen voor het verbeteren van SQL code kwaliteit
        /// </summary>
        /// <param name="solutionPath">Pad naar de solution</param>
        /// <param name="outputFilePath">Pad waar de aanbevelingen geschreven moeten worden</param>
        public static void GenerateSqlRecommendations(string solutionPath, string outputFilePath)
        {
            try
            {
                Console.WriteLine("Genereren van SQL aanbevelingen...");
                
                var sqlVerifier = new SqlCodeVerifier(solutionPath);
                var recommendations = sqlVerifier.GenerateRecommendations();
                
                string report = $"SQL Code Kwaliteitsaanbevelingen ({DateTime.Now}):\n\n";
                
                if (recommendations.Count > 0)
                {
                    foreach (var recommendation in recommendations)
                    {
                        report += $"- {recommendation}\n";
                    }
                }
                else
                {
                    report += "Geen specifieke aanbevelingen gevonden.";
                }
                
                File.WriteAllText(outputFilePath, report);
                
                Console.WriteLine($"SQL aanbevelingen zijn geschreven naar: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij het genereren van SQL aanbevelingen: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Hulpmethode om de solution directory te vinden vanaf een bestandspad binnen het project
        /// </summary>
        /// <param name="startPath">Een pad binnen het project</param>
        /// <returns>Het pad naar de solution directory of null als niet gevonden</returns>
        public static string FindSolutionPath(string startPath)
        {
            DirectoryInfo dir = new DirectoryInfo(startPath);
            
            while (dir != null)
            {
                if (Directory.GetFiles(dir.FullName, "*.sln").Length > 0)
                {
                    return dir.FullName;
                }
                
                dir = dir.Parent;
            }
            
            return null;
        }
    }
} 