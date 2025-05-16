using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BenchmarkTool.ClassLibrary.CodeQuality
{
    /// <summary>
    /// Hulpmiddel voor het verifiëren dat SQL code alleen in de Class Library voorkomt
    /// en voor het bewaken van de codekwaliteit van SQL queries
    /// </summary>
    public class SqlCodeVerifier
    {
        private readonly string _solutionPath;
        private readonly string[] _sqlKeywords = new string[]
        {
            "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE TABLE", "DROP TABLE", 
            "ALTER TABLE", "JOIN", "WHERE", "GROUP BY", "ORDER BY", "HAVING", 
            "EXEC ", "EXECUTE", "PROCEDURE", "FUNCTION", "TRIGGER"
        };

        // Bestandspatronen die geanalyseerd moeten worden
        private readonly string[] _filePatterns = new string[] { "*.cs", "*.xaml", "*.xaml.cs" };

        // Mappen om te negeren in de analyse
        private readonly string[] _ignoreFolders = new string[] { "bin", "obj", "packages", "node_modules", ".vs" };

        public SqlCodeVerifier(string solutionPath)
        {
            _solutionPath = solutionPath ?? throw new ArgumentNullException(nameof(solutionPath));
        }

        /// <summary>
        /// Controleert of SQL code alleen in de juiste bestanden in de Class Library voorkomt
        /// </summary>
        /// <returns>Een lijst van problemen, leeg als er geen problemen zijn</returns>
        public List<SqlCodeViolation> VerifySqlCodeIsolation()
        {
            var violations = new List<SqlCodeViolation>();
            
            // Pad naar de AdminApp en CompanyApp projecten
            string adminAppPath = Path.Combine(_solutionPath, "BenchmarkTool.AdminApp");
            string companyAppPath = Path.Combine(_solutionPath, "BenchmarkTool.CompanyApp");
            
            // Controleer beide WPF projecten op SQL code
            violations.AddRange(ScanFolder(adminAppPath, true));
            violations.AddRange(ScanFolder(companyAppPath, true));
            
            // Controleer Class Library, maar alleen buiten de Data map
            string classLibraryPath = Path.Combine(_solutionPath, "BenchmarkTool.ClassLibrary");
            string dataFolderPath = Path.Combine(classLibraryPath, "Data");
            
            foreach (var filePath in GetFiles(classLibraryPath, _filePatterns))
            {
                // Sla bestanden in de Data map over
                if (!filePath.StartsWith(dataFolderPath, StringComparison.OrdinalIgnoreCase))
                {
                    violations.AddRange(ScanFile(filePath, false));
                }
            }
            
            return violations;
        }

        /// <summary>
        /// Analyseert de kwaliteit van SQL queries in de Class Library
        /// </summary>
        /// <returns>Een kwaliteitsrapport over de SQL queries</returns>
        public SqlCodeQualityReport AnalyzeSqlCodeQuality()
        {
            var report = new SqlCodeQualityReport();
            string dataFolderPath = Path.Combine(_solutionPath, "BenchmarkTool.ClassLibrary", "Data");
            
            foreach (var filePath in GetFiles(dataFolderPath, new[] { "*.cs" }))
            {
                AnalyzeFileForSqlQuality(filePath, report);
            }
            
            return report;
        }

        /// <summary>
        /// Genereert aanbevelingen voor het verbeteren van SQL code
        /// </summary>
        /// <returns>Een lijst van aanbevelingen</returns>
        public List<string> GenerateRecommendations()
        {
            var recommendations = new List<string>();
            var report = AnalyzeSqlCodeQuality();
            
            if (report.LongQueries.Count > 0)
            {
                recommendations.Add($"Er zijn {report.LongQueries.Count} lange SQL queries gevonden. Overweeg deze op te splitsen in kleinere, meer beheerbare queries.");
            }
            
            if (report.ComplexQueries.Count > 0)
            {
                recommendations.Add($"Er zijn {report.ComplexQueries.Count} complexe SQL queries gevonden. Overweeg deze te vereenvoudigen of te documenteren.");
            }
            
            if (report.ParametersCount < (report.StringConcatenationCount / 2))
            {
                recommendations.Add("Er zijn mogelijk SQL injectie risico's door string concatenatie. Gebruik altijd geparameteriseerde queries.");
            }
            
            if (report.HardcodedValues.Count > 0)
            {
                recommendations.Add($"Er zijn {report.HardcodedValues.Count} hardcoded waarden gevonden in SQL queries. Overweeg deze te parametriseren.");
            }
            
            if (report.HasSelectStar)
            {
                recommendations.Add("SELECT * wordt gebruikt in queries. Specificeer altijd expliciet de benodigde kolommen voor betere prestaties.");
            }
            
            return recommendations;
        }

        #region Private helper methods
        
        private List<SqlCodeViolation> ScanFolder(string folderPath, bool isAppProject)
        {
            var violations = new List<SqlCodeViolation>();
            
            foreach (var filePath in GetFiles(folderPath, _filePatterns))
            {
                violations.AddRange(ScanFile(filePath, isAppProject));
            }
            
            return violations;
        }
        
        private List<SqlCodeViolation> ScanFile(string filePath, bool isAppProject)
        {
            var violations = new List<SqlCodeViolation>();
            string fileName = Path.GetFileName(filePath);
            string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            
            try
            {
                string content = File.ReadAllText(filePath);
                
                foreach (var keyword in _sqlKeywords)
                {
                    // Zoek naar SQL keywords in de code, pas regex toe om false positives te voorkomen
                    var matches = Regex.Matches(content, $@"(?<![a-zA-Z0-9_])({keyword})(?![a-zA-Z0-9_])", RegexOptions.IgnoreCase);
                    
                    foreach (Match match in matches)
                    {
                        // Als dit een WPF-project is of een bestand buiten de Data map in ClassLibrary,
                        // dan is SQL code hier niet toegestaan
                        if (isAppProject)
                        {
                            violations.Add(new SqlCodeViolation
                            {
                                FilePath = filePath,
                                LineNumber = GetLineNumber(content, match.Index),
                                SqlKeyword = match.Value,
                                ViolationType = "SQL code in WPF-applicatie",
                                Recommendation = "Verplaats alle SQL code naar de ClassLibrary.Data namespace"
                            });
                        }
                        else // ClassLibrary maar buiten Data map
                        {
                            violations.Add(new SqlCodeViolation
                            {
                                FilePath = filePath,
                                LineNumber = GetLineNumber(content, match.Index),
                                SqlKeyword = match.Value,
                                ViolationType = "SQL code buiten Data namespace",
                                Recommendation = "Verplaats SQL code naar een passende klasse in de ClassLibrary.Data namespace"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                violations.Add(new SqlCodeViolation
                {
                    FilePath = filePath,
                    LineNumber = 0,
                    SqlKeyword = "",
                    ViolationType = "Fout bij scannen bestand",
                    Recommendation = $"Fout: {ex.Message}"
                });
            }
            
            return violations;
        }
        
        private void AnalyzeFileForSqlQuality(string filePath, SqlCodeQualityReport report)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                string fileName = Path.GetFileName(filePath);
                
                // Vind SQL queries in string literals
                var queryMatches = Regex.Matches(content, @"([""])(?:\\\1|.)*?\1");
                
                foreach (Match match in queryMatches)
                {
                    string query = match.Value.Trim('"', '\'');
                    
                    // Negeer lege strings of korte strings die waarschijnlijk geen queries zijn
                    if (query.Length < 10 || !ContainsSqlKeyword(query))
                        continue;
                    
                    report.TotalQueries++;
                    
                    // Controleer op lange queries
                    if (query.Length > 500)
                    {
                        report.LongQueries.Add(new SqlQueryInfo
                        {
                            FilePath = filePath,
                            LineNumber = GetLineNumber(content, match.Index),
                            Query = query.Length > 100 ? query.Substring(0, 100) + "..." : query
                        });
                    }
                    
                    // Controleer op complexe queries (gebruik van subqueries, meerdere JOINs, etc.)
                    if (Regex.IsMatch(query, @"\(SELECT") || Regex.Matches(query, @"\bJOIN\b").Count > 2)
                    {
                        report.ComplexQueries.Add(new SqlQueryInfo
                        {
                            FilePath = filePath,
                            LineNumber = GetLineNumber(content, match.Index),
                            Query = query.Length > 100 ? query.Substring(0, 100) + "..." : query
                        });
                    }
                    
                    // Controleer op SELECT *
                    if (Regex.IsMatch(query, @"SELECT\s+\*\s+FROM", RegexOptions.IgnoreCase))
                    {
                        report.HasSelectStar = true;
                    }
                    
                    // Controleer op hardcoded waarden
                    var hardcodedMatches = Regex.Matches(query, @"(?<=WHERE|=|>|<|\bIN\b|\bVALUES\b)\s*'[^']+'", RegexOptions.IgnoreCase);
                    foreach (Match hardcodedMatch in hardcodedMatches)
                    {
                        report.HardcodedValues.Add(new SqlQueryInfo
                        {
                            FilePath = filePath,
                            LineNumber = GetLineNumber(content, match.Index),
                            Query = hardcodedMatch.Value
                        });
                    }
                }
                
                // Tel gebruik van parameters vs string concatenatie
                report.ParametersCount += Regex.Matches(content, @"new\s+SqlParameter").Count;
                report.StringConcatenationCount += Regex.Matches(content, @"string\s+query\s*=.*\+").Count;
            }
            catch (Exception)
            {
                // Logging zou hier moeten gebeuren, maar we slaan fouten nu over
            }
        }
        
        private bool ContainsSqlKeyword(string text)
        {
            return _sqlKeywords.Any(keyword => Regex.IsMatch(text, $@"\b{keyword}\b", RegexOptions.IgnoreCase));
        }
        
        private int GetLineNumber(string content, int position)
        {
            int lineNumber = 1;
            for (int i = 0; i < position; i++)
            {
                if (content[i] == '\n')
                    lineNumber++;
            }
            return lineNumber;
        }
        
        private IEnumerable<string> GetFiles(string folderPath, string[] patterns)
        {
            if (!Directory.Exists(folderPath))
                yield break;
            
            foreach (var pattern in patterns)
            {
                foreach (var file in Directory.EnumerateFiles(folderPath, pattern, SearchOption.AllDirectories))
                {
                    // Skip files in ignored folders
                    if (_ignoreFolders.Any(ignore => file.Contains(Path.DirectorySeparatorChar + ignore + Path.DirectorySeparatorChar)))
                        continue;
                    
                    yield return file;
                }
            }
        }
        
        #endregion
    }

    /// <summary>
    /// Vertegenwoordigt een SQL code probleem in de codebase
    /// </summary>
    public class SqlCodeViolation
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string SqlKeyword { get; set; }
        public string ViolationType { get; set; }
        public string Recommendation { get; set; }
        
        public override string ToString()
        {
            return $"{FilePath}:{LineNumber} - {ViolationType}: {SqlKeyword} - {Recommendation}";
        }
    }

    /// <summary>
    /// Informatie over een SQL query voor rapportage
    /// </summary>
    public class SqlQueryInfo
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string Query { get; set; }
        
        public override string ToString()
        {
            return $"{FilePath}:{LineNumber} - {Query}";
        }
    }

    /// <summary>
    /// Rapport over de kwaliteit van SQL code in het project
    /// </summary>
    public class SqlCodeQualityReport
    {
        public int TotalQueries { get; set; }
        public List<SqlQueryInfo> LongQueries { get; set; } = new List<SqlQueryInfo>();
        public List<SqlQueryInfo> ComplexQueries { get; set; } = new List<SqlQueryInfo>();
        public List<SqlQueryInfo> HardcodedValues { get; set; } = new List<SqlQueryInfo>();
        public bool HasSelectStar { get; set; }
        public int ParametersCount { get; set; }
        public int StringConcatenationCount { get; set; }
        
        public override string ToString()
        {
            return $"Totaal aantal queries: {TotalQueries}, Lange queries: {LongQueries.Count}, " +
                   $"Complexe queries: {ComplexQueries.Count}, Hardcoded waarden: {HardcodedValues.Count}, " +
                   $"SELECT * gebruikt: {HasSelectStar}, Parameters/Concatenaties: {ParametersCount}/{StringConcatenationCount}";
        }
    }
} 