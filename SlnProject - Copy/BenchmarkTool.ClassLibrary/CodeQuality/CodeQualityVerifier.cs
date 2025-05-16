using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BenchmarkTool.ClassLibrary.CodeQuality
{
    /// <summary>
    /// Hulpmiddel voor het verifiëren en verbeteren van de codekwaliteit in het project
    /// </summary>
    public class CodeQualityVerifier
    {
        private readonly string _solutionPath;
        private readonly SqlCodeVerifier _sqlVerifier;

        public CodeQualityVerifier(string solutionPath)
        {
            _solutionPath = solutionPath ?? throw new ArgumentNullException(nameof(solutionPath));
            _sqlVerifier = new SqlCodeVerifier(solutionPath);
        }

        /// <summary>
        /// Voert een volledige kwaliteitscontrole uit op het project
        /// </summary>
        /// <returns>Een rapport met bevindingen en aanbevelingen</returns>
        public CodeQualityReport VerifyCodeQuality()
        {
            var report = new CodeQualityReport();

            // Controleer SQL code isolatie
            report.SqlCodeViolations = _sqlVerifier.VerifySqlCodeIsolation();
            
            // Analyseer SQL code kwaliteit
            report.SqlQualityReport = _sqlVerifier.AnalyzeSqlCodeQuality();
            
            // Controleer bestandsgroottes
            report.LargeFiles = FindLargeFiles();
            
            // Controleer duplicatie
            report.PotentialDuplication = FindPotentialDuplication();
            
            // Controleer .NET naamconventies
            report.NamingViolations = VerifyNamingConventions();
            
            // Genereer aanbevelingen
            report.SqlRecommendations = _sqlVerifier.GenerateRecommendations();
            
            return report;
        }

        /// <summary>
        /// Vindt bestanden die groter zijn dan de aanbevolen grootte (200-300 regels)
        /// </summary>
        public List<FileInfo> FindLargeFiles(int threshold = 200)
        {
            var largeFiles = new List<FileInfo>();
            string[] projectDirs = new[]
            {
                Path.Combine(_solutionPath, "BenchmarkTool.ClassLibrary"),
                Path.Combine(_solutionPath, "BenchmarkTool.AdminApp"),
                Path.Combine(_solutionPath, "BenchmarkTool.CompanyApp")
            };
            
            foreach (var dir in projectDirs.Where(Directory.Exists))
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories))
                {
                    // Sla gegenereerde bestanden over
                    if (file.Contains("\\obj\\") || file.Contains("\\bin\\") || file.Contains(".designer.cs"))
                        continue;
                    
                    int lineCount = File.ReadAllLines(file).Length;
                    if (lineCount > threshold)
                    {
                        largeFiles.Add(new FileInfo
                        {
                            FilePath = file,
                            LineCount = lineCount,
                            Recommendation = $"Bestand is {lineCount} regels lang, overweeg op te splitsen in kleinere klassen/bestanden."
                        });
                    }
                }
            }
            
            return largeFiles.OrderByDescending(f => f.LineCount).ToList();
        }

        /// <summary>
        /// Vindt potentiële code duplicatie
        /// </summary>
        public List<DuplicationInfo> FindPotentialDuplication()
        {
            var duplications = new List<DuplicationInfo>();
            var methodSignatures = new Dictionary<string, string>();
            string[] projectDirs = new[]
            {
                Path.Combine(_solutionPath, "BenchmarkTool.ClassLibrary"),
                Path.Combine(_solutionPath, "BenchmarkTool.AdminApp"),
                Path.Combine(_solutionPath, "BenchmarkTool.CompanyApp")
            };
            
            foreach (var dir in projectDirs.Where(Directory.Exists))
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories))
                {
                    // Sla gegenereerde bestanden over
                    if (file.Contains("\\obj\\") || file.Contains("\\bin\\") || file.Contains(".designer.cs"))
                        continue;
                    
                    string content = File.ReadAllText(file);
                    
                    // Zoek naar methode-handtekeningen
                    var methodMatches = Regex.Matches(content, @"(public|private|protected|internal)\s+(?:static\s+)?\w+\s+(\w+)\s*\([^)]*\)");
                    foreach (Match match in methodMatches)
                    {
                        string methodSignature = match.Value;
                        string methodName = match.Groups[2].Value;
                        
                        // Sla getters/setters en kleine methoden over
                        if (methodName.StartsWith("get_") || methodName.StartsWith("set_") || 
                            methodName.Length <= 3 || methodName == "Main")
                            continue;
                        
                        if (methodSignatures.ContainsKey(methodName))
                        {
                            duplications.Add(new DuplicationInfo
                            {
                                OriginalFilePath = methodSignatures[methodName],
                                DuplicateFilePath = file,
                                DuplicateText = methodName,
                                Recommendation = $"Methode '{methodName}' komt mogelijk meerdere keren voor. Overweeg hergebruik of een gemeenschappelijke basisklasse."
                            });
                        }
                        else
                        {
                            methodSignatures[methodName] = file;
                        }
                    }
                }
            }
            
            return duplications;
        }

        /// <summary>
        /// Controleert of naamgevingsconventies worden gevolgd
        /// </summary>
        public List<NamingViolation> VerifyNamingConventions()
        {
            var violations = new List<NamingViolation>();
            string[] projectDirs = new[]
            {
                Path.Combine(_solutionPath, "BenchmarkTool.ClassLibrary"),
                Path.Combine(_solutionPath, "BenchmarkTool.AdminApp"),
                Path.Combine(_solutionPath, "BenchmarkTool.CompanyApp")
            };
            
            foreach (var dir in projectDirs.Where(Directory.Exists))
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories))
                {
                    // Sla gegenereerde bestanden over
                    if (file.Contains("\\obj\\") || file.Contains("\\bin\\") || file.Contains(".designer.cs"))
                        continue;
                    
                    string content = File.ReadAllText(file);
                    
                    // Controleer klasse definities (PascalCase)
                    var classMatches = Regex.Matches(content, @"class\s+(\w+)");
                    foreach (Match match in classMatches)
                    {
                        string className = match.Groups[1].Value;
                        if (!Regex.IsMatch(className, @"^[A-Z][a-zA-Z0-9]*$"))
                        {
                            violations.Add(new NamingViolation
                            {
                                FilePath = file,
                                ElementType = "Klasse",
                                ElementName = className,
                                Convention = "PascalCase",
                                Recommendation = $"Klasse '{className}' moet beginnen met een hoofdletter en PascalCase volgen."
                            });
                        }
                    }
                    
                    // Controleer methode definities (PascalCase)
                    var methodMatches = Regex.Matches(content, @"(public|private|protected|internal)\s+(?:static\s+)?\w+\s+(\w+)\s*\(");
                    foreach (Match match in methodMatches)
                    {
                        string methodName = match.Groups[2].Value;
                        if (!Regex.IsMatch(methodName, @"^[A-Z][a-zA-Z0-9]*$"))
                        {
                            violations.Add(new NamingViolation
                            {
                                FilePath = file,
                                ElementType = "Methode",
                                ElementName = methodName,
                                Convention = "PascalCase",
                                Recommendation = $"Methode '{methodName}' moet beginnen met een hoofdletter en PascalCase volgen."
                            });
                        }
                    }
                    
                    // Controleer veld definities (camelCase of _camelCase voor private)
                    var fieldMatches = Regex.Matches(content, @"(private|protected)\s+(?:readonly\s+)?\w+\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*[;=]");
                    foreach (Match match in fieldMatches)
                    {
                        string fieldName = match.Groups[2].Value;
                        if (!fieldName.StartsWith("_") && !Regex.IsMatch(fieldName, @"^[a-z][a-zA-Z0-9]*$"))
                        {
                            violations.Add(new NamingViolation
                            {
                                FilePath = file,
                                ElementType = "Veld",
                                ElementName = fieldName,
                                Convention = "camelCase of _camelCase",
                                Recommendation = $"Privaat/protected veld '{fieldName}' moet camelCase zijn of beginnen met underscore."
                            });
                        }
                    }
                    
                    // Controleer property definities (PascalCase)
                    var propertyMatches = Regex.Matches(content, @"(public|private|protected|internal)\s+\w+\s+(\w+)\s*\{");
                    foreach (Match match in methodMatches)
                    {
                        string propertyName = match.Groups[2].Value;
                        if (!Regex.IsMatch(propertyName, @"^[A-Z][a-zA-Z0-9]*$"))
                        {
                            violations.Add(new NamingViolation
                            {
                                FilePath = file,
                                ElementType = "Property",
                                ElementName = propertyName,
                                Convention = "PascalCase",
                                Recommendation = $"Property '{propertyName}' moet beginnen met een hoofdletter en PascalCase volgen."
                            });
                        }
                    }
                    
                    // Controleer parameter definities (camelCase)
                    var parameterMatches = Regex.Matches(content, @"\(\s*(?:[^,\(\)]+\s+(\w+)(?:\s*=\s*[^,\(\)]+)?(?:\s*,\s*)?)+\s*\)");
                    foreach (Match match in parameterMatches)
                    {
                        foreach (var groupMatch in match.Groups[1].Captures)
                        {
                            string paramName = groupMatch.ToString();
                            if (!Regex.IsMatch(paramName, @"^[a-z][a-zA-Z0-9]*$"))
                            {
                                violations.Add(new NamingViolation
                                {
                                    FilePath = file,
                                    ElementType = "Parameter",
                                    ElementName = paramName,
                                    Convention = "camelCase",
                                    Recommendation = $"Parameter '{paramName}' moet beginnen met een kleine letter en camelCase volgen."
                                });
                            }
                        }
                    }
                    
                    // Controleer UI control naamgeving (Hongaarse notatie, bv. txtUsername)
                    if (file.Contains("AdminApp") || file.Contains("CompanyApp"))
                    {
                        var controlMatches = Regex.Matches(content, @"(TextBox|Button|ComboBox|CheckBox|RadioButton|Label|PasswordBox|Image|ListBox)\s+(\w+)\s*[;=]");
                        foreach (Match match in controlMatches)
                        {
                            string controlType = match.Groups[1].Value.ToLower();
                            string controlName = match.Groups[2].Value;
                            string expectedPrefix = GetHungarianPrefix(controlType);
                            
                            if (expectedPrefix != null && !controlName.StartsWith(expectedPrefix))
                            {
                                violations.Add(new NamingViolation
                                {
                                    FilePath = file,
                                    ElementType = "UI Control",
                                    ElementName = controlName,
                                    Convention = "Hongaarse notatie",
                                    Recommendation = $"UI control '{controlName}' van type '{match.Groups[1].Value}' moet beginnen met '{expectedPrefix}'."
                                });
                            }
                        }
                    }
                }
            }
            
            return violations;
        }

        private string GetHungarianPrefix(string controlType)
        {
            switch (controlType.ToLower())
            {
                case "textbox": return "txt";
                case "button": return "btn";
                case "combobox": return "cmb";
                case "checkbox": return "chk";
                case "radiobutton": return "rdo";
                case "label": return "lbl";
                case "passwordbox": return "pwd";
                case "image": return "img";
                case "listbox": return "lst";
                default: return null;
            }
        }
    }

    /// <summary>
    /// Informatie over een bestand
    /// </summary>
    public class FileInfo
    {
        public string FilePath { get; set; }
        public int LineCount { get; set; }
        public string Recommendation { get; set; }
        
        public override string ToString()
        {
            return $"{FilePath} ({LineCount} regels) - {Recommendation}";
        }
    }

    /// <summary>
    /// Informatie over mogelijke code duplicatie
    /// </summary>
    public class DuplicationInfo
    {
        public string OriginalFilePath { get; set; }
        public string DuplicateFilePath { get; set; }
        public string DuplicateText { get; set; }
        public string Recommendation { get; set; }
        
        public override string ToString()
        {
            return $"Mogelijke duplicatie van '{DuplicateText}' gevonden in {OriginalFilePath} en {DuplicateFilePath} - {Recommendation}";
        }
    }

    /// <summary>
    /// Informatie over een schending van naamgevingsconventies
    /// </summary>
    public class NamingViolation
    {
        public string FilePath { get; set; }
        public string ElementType { get; set; }
        public string ElementName { get; set; }
        public string Convention { get; set; }
        public string Recommendation { get; set; }
        
        public override string ToString()
        {
            return $"{FilePath} - {ElementType} '{ElementName}' voldoet niet aan {Convention}: {Recommendation}";
        }
    }

    /// <summary>
    /// Volledig rapport over de codekwaliteit
    /// </summary>
    public class CodeQualityReport
    {
        public List<SqlCodeViolation> SqlCodeViolations { get; set; } = new List<SqlCodeViolation>();
        public SqlCodeQualityReport SqlQualityReport { get; set; }
        public List<string> SqlRecommendations { get; set; } = new List<string>();
        public List<FileInfo> LargeFiles { get; set; } = new List<FileInfo>();
        public List<DuplicationInfo> PotentialDuplication { get; set; } = new List<DuplicationInfo>();
        public List<NamingViolation> NamingViolations { get; set; } = new List<NamingViolation>();
        
        public bool HasIssues => 
            SqlCodeViolations.Count > 0 || 
            LargeFiles.Count > 0 || 
            PotentialDuplication.Count > 0 || 
            NamingViolations.Count > 0 ||
            (SqlQualityReport != null && (
                SqlQualityReport.LongQueries.Count > 0 || 
                SqlQualityReport.ComplexQueries.Count > 0 || 
                SqlQualityReport.HardcodedValues.Count > 0 ||
                SqlQualityReport.HasSelectStar));
        
        public override string ToString()
        {
            string summary = "Code Kwaliteit Rapport:\n\n";
            
            if (SqlCodeViolations.Count > 0)
            {
                summary += $"SQL Code Schendingen: {SqlCodeViolations.Count}\n";
                foreach (var violation in SqlCodeViolations.Take(5))
                {
                    summary += $"  - {violation}\n";
                }
                if (SqlCodeViolations.Count > 5)
                {
                    summary += $"  - En {SqlCodeViolations.Count - 5} meer...\n";
                }
                summary += "\n";
            }
            
            if (SqlQualityReport != null)
            {
                summary += $"SQL Kwaliteit: {SqlQualityReport}\n\n";
            }
            
            if (SqlRecommendations.Count > 0)
            {
                summary += "SQL Aanbevelingen:\n";
                foreach (var recommendation in SqlRecommendations)
                {
                    summary += $"  - {recommendation}\n";
                }
                summary += "\n";
            }
            
            if (LargeFiles.Count > 0)
            {
                summary += $"Grote Bestanden: {LargeFiles.Count}\n";
                foreach (var file in LargeFiles.Take(5))
                {
                    summary += $"  - {file}\n";
                }
                if (LargeFiles.Count > 5)
                {
                    summary += $"  - En {LargeFiles.Count - 5} meer...\n";
                }
                summary += "\n";
            }
            
            if (PotentialDuplication.Count > 0)
            {
                summary += $"Mogelijke Duplicatie: {PotentialDuplication.Count}\n";
                foreach (var duplication in PotentialDuplication.Take(5))
                {
                    summary += $"  - {duplication}\n";
                }
                if (PotentialDuplication.Count > 5)
                {
                    summary += $"  - En {PotentialDuplication.Count - 5} meer...\n";
                }
                summary += "\n";
            }
            
            if (NamingViolations.Count > 0)
            {
                summary += $"Naamgevingsschendingen: {NamingViolations.Count}\n";
                foreach (var violation in NamingViolations.Take(5))
                {
                    summary += $"  - {violation}\n";
                }
                if (NamingViolations.Count > 5)
                {
                    summary += $"  - En {NamingViolations.Count - 5} meer...\n";
                }
            }
            
            return summary;
        }
    }
} 