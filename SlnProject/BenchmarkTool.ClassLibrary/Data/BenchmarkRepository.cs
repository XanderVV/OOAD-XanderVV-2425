using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkTool.ClassLibrary.Models;
using Microsoft.Data.SqlClient;

namespace BenchmarkTool.ClassLibrary.Data
{
    /// <summary>
    /// Repository voor het beheren van benchmark data
    /// </summary>
    public class BenchmarkRepository : DatabaseRepository, IBenchmarkRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BenchmarkRepository"/> class with a default DatabaseHelper.
        /// </summary>
        public BenchmarkRepository() 
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BenchmarkRepository"/> class with an explicit DatabaseHelper.
        /// </summary>
        /// <param name="dbHelper">De te gebruiken DatabaseHelper</param>
        public BenchmarkRepository(DatabaseHelper dbHelper) 
            : base(dbHelper)
        {
        }

        /// <summary>
        /// Haalt benchmark data op van andere bedrijven op basis van criteria
        /// </summary>
        /// <param name="currentCompanyId">ID van het huidige bedrijf (om uit te sluiten)</param>
        /// <param name="year">Jaar waarvoor data opgehaald moet worden</param>
        /// <param name="naceFilter">Filter voor NACE-code (kan null zijn voor geen filter)</param>
        /// <param name="groupingLevel">Niveau van NACE-code groepering</param>
        /// <param name="selectedIndicators">Lijst van geselecteerde indicatoren (Questions.id, Costtypes.type, 'fte')</param>
        /// <returns>Lijst van benchmark data</returns>
        public List<BenchmarkData> GetBenchmarkData(
            int currentCompanyId,
            int year,
            string naceFilter,
            NaceGroupingLevel groupingLevel,
            List<string> selectedIndicators)
        {
            var result = new List<BenchmarkData>();

            try
            {
                // Ophalen van fte data indien geselecteerd
                if (selectedIndicators.Contains("fte"))
                {
                    result.AddRange(GetFteBenchmarkData(currentCompanyId, year, naceFilter, groupingLevel));
                }

                // Ophalen van kosten data op basis van geselecteerde kosttypes
                var selectedCostTypes = selectedIndicators
                    .Where(i => i.StartsWith("cost_"))
                    .Select(i => i.Substring(5))
                    .ToList();

                if (selectedCostTypes.Any())
                {
                    result.AddRange(GetCostsBenchmarkData(currentCompanyId, year, naceFilter, groupingLevel, selectedCostTypes));
                }

                // Ophalen van antwoorden data op basis van geselecteerde vragen
                var selectedQuestionIds = selectedIndicators
                    .Where(i => i.StartsWith("question_"))
                    .Select(i => Convert.ToInt32(i.Substring(9)))
                    .ToList();

                if (selectedQuestionIds.Any())
                {
                    result.AddRange(GetAnswersBenchmarkData(currentCompanyId, year, naceFilter, groupingLevel, selectedQuestionIds));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen benchmark data: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Haalt eigen data op van een bedrijf voor vergelijking met de benchmark
        /// </summary>
        /// <param name="companyId">ID van het bedrijf</param>
        /// <param name="year">Jaar waarvoor data opgehaald moet worden</param>
        /// <param name="selectedIndicators">Lijst van geselecteerde indicatoren (Questions.id, Costtypes.type, 'fte')</param>
        /// <returns>Lijst van eigen data</returns>
        public List<BenchmarkData> GetOwnData(
            int companyId,
            int year,
            List<string> selectedIndicators)
        {
            var result = new List<BenchmarkData>();

            try
            {
                // Ophalen van jaarrapport voor het bedrijf
                string jaarrapportQuery = @"
                    SELECT y.id, y.year, y.fte, c.nacecode_code 
                    FROM Yearreports y
                    JOIN Companies c ON y.company_id = c.id
                    WHERE y.company_id = @CompanyId AND y.year = @Year";

                SqlParameter[] jaarrapportParams = new SqlParameter[] { 
                    new SqlParameter("@CompanyId", companyId), 
                    new SqlParameter("@Year", year) 
                };

                // Get yearreport data using ExecuteReader
                int jaarrapportId = 0;
                string naceCode = "";
                decimal fte = 0;

                DbHelper.ExecuteReader(jaarrapportQuery, reader 
                => 
                {
                    if (reader.Read())
                    {
                        jaarrapportId = Convert.ToInt32(reader["id"]);
                        naceCode = reader["nacecode_code"].ToString();
                        fte = Convert.ToDecimal(reader["fte"]);
                    }
                }, jaarrapportParams);

                if (jaarrapportId == 0)
                {
                    return result; // Geen jaarrapport gevonden voor dit jaar
                }

                // Ophalen van fte data indien geselecteerd
                if (selectedIndicators.Contains("fte"))
                {
                    result.Add(new BenchmarkData
                    {
                        Indicator = "fte",
                        IndicatorType = "fte",
                        Waarde = fte,
                        Jaar = year,
                        NaceCode = naceCode
                    });
                }

                // Ophalen van kosten data op basis van geselecteerde kosttypes
                var selectedCostTypes = selectedIndicators
                    .Where(i => i.StartsWith("cost_"))
                    .Select(i => i.Substring(5))
                    .ToList();

                if (selectedCostTypes.Any())
                {
                    string placeholders = string.Join(
                        ",", 
                        selectedCostTypes.Select((_, i) => $"@CostType{i}"));
                    
                    string kostenQuery = $@"
                        SELECT c.value, c.costtype_type 
                        FROM Costs c
                        WHERE c.yearreport_id = @YearreportId 
                        AND c.costtype_type IN ({placeholders})";

                    List<SqlParameter> kostenParams = new List<SqlParameter>
                    {
                        new SqlParameter("@YearreportId", jaarrapportId)
                    };

                    for (int i = 0; i < selectedCostTypes.Count; i++)
                    {
                        kostenParams.Add(new SqlParameter($"@CostType{i}", selectedCostTypes[i]));
                    }

                    var kostenData = DbHelper.ExecuteQuery(kostenQuery, kostenParams.ToArray());

                    foreach (var row in kostenData)
                    {
                        result.Add(new BenchmarkData
                        {
                            Indicator = "cost_" + row["costtype_type"].ToString(),
                            IndicatorType = "cost",
                            Waarde = Convert.ToDecimal(row["value"]),
                            Jaar = year,
                            NaceCode = naceCode
                        });
                    }
                }

                // Ophalen van antwoorden data op basis van geselecteerde vragen
                var selectedQuestionIds = selectedIndicators
                    .Where(i => i.StartsWith("question_"))
                    .Select(i => Convert.ToInt32(i.Substring(9)))
                    .ToList();

                if (selectedQuestionIds.Any())
                {
                    string placeholders = string.Join(
                        ",",
                        selectedQuestionIds.Select((_, i) => $"@QuestionId{i}"));
                    
                    string antwoordenQuery = $@"
                        SELECT a.value, a.question_id 
                        FROM Answers a
                        WHERE a.yearreport_id = @YearreportId 
                        AND a.question_id IN ({placeholders})";

                    List<SqlParameter> antwoordenParams = new List<SqlParameter>
                    {
                        new SqlParameter("@YearreportId", jaarrapportId)
                    };

                    for (int i = 0; i < selectedQuestionIds.Count; i++)
                    {
                        antwoordenParams.Add(new SqlParameter($"@QuestionId{i}", selectedQuestionIds[i]));
                    }

                    var antwoordenData = DbHelper.ExecuteQuery(antwoordenQuery, antwoordenParams.ToArray());

                    foreach (var row in antwoordenData)
                    {
                        decimal waarde;
                        if (decimal.TryParse(row["value"].ToString(), out waarde))
                        {
                            result.Add(new BenchmarkData
                            {
                                Indicator = "question_" + row["question_id"].ToString(),
                                IndicatorType = "question",
                                Waarde = waarde,
                                Jaar = year,
                                NaceCode = naceCode
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen eigen data: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Haalt FTE benchmark data op
        /// </summary>
        /// <param name="currentCompanyId">ID van het huidige bedrijf (om uit te sluiten)</param>
        /// <param name="year">Jaar waarvoor data opgehaald moet worden</param>
        /// <param name="naceFilter">Filter voor NACE-code (kan null zijn voor geen filter)</param>
        /// <param name="groupingLevel">Niveau van NACE-code groepering</param>
        /// <returns>Lijst van FTE benchmark data</returns>
        private List<BenchmarkData> GetFteBenchmarkData(
            int currentCompanyId,
            int year,
            string naceFilter,
            NaceGroupingLevel groupingLevel)
        {
            var result = new List<BenchmarkData>();

            try
            {
                string naceGroupingField = GetNaceGroupingField(groupingLevel);
                string naceFilterCondition = string.IsNullOrEmpty(naceFilter) 
                    ? "" 
                    : $"AND {naceGroupingField} = @NaceFilter";

                string query = $@"
                    SELECT 
                        y.fte, 
                        y.year,
                        {naceGroupingField} as nace_group
                    FROM Yearreports y
                    JOIN Companies c ON y.company_id = c.id
                    WHERE y.company_id != @CurrentCompanyId 
                    AND y.year = @Year
                    {naceFilterCondition}";

                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@CurrentCompanyId", currentCompanyId),
                    new SqlParameter("@Year", year)
                };

                if (!string.IsNullOrEmpty(naceFilter))
                {
                    parameters.Add(new SqlParameter("@NaceFilter", naceFilter));
                }

                var results = DbHelper.ExecuteQuery(query, parameters.ToArray());

                foreach (var row in results)
                {
                    result.Add(new BenchmarkData
                    {
                        Indicator = "fte",
                        IndicatorType = "fte",
                        Waarde = Convert.ToDecimal(row["fte"]),
                        Jaar = Convert.ToInt32(row["year"]),
                        NaceCode = row["nace_group"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen FTE benchmark data: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Haalt costs benchmark data op
        /// </summary>
        /// <param name="currentCompanyId">ID van het huidige bedrijf (om uit te sluiten)</param>
        /// <param name="year">Jaar waarvoor data opgehaald moet worden</param>
        /// <param name="naceFilter">Filter voor NACE-code (kan null zijn voor geen filter)</param>
        /// <param name="groupingLevel">Niveau van NACE-code groepering</param>
        /// <param name="costTypes">Lijst van kosttypes</param>
        /// <returns>Lijst van costs benchmark data</returns>
        private List<BenchmarkData> GetCostsBenchmarkData(
            int currentCompanyId,
            int year,
            string naceFilter,
            NaceGroupingLevel groupingLevel,
            List<string> costTypes)
        {
            var result = new List<BenchmarkData>();

            try
            {
                string naceGroupingField = GetNaceGroupingField(groupingLevel);
                string naceFilterCondition = string.IsNullOrEmpty(naceFilter) 
                    ? "" 
                    : $"AND {naceGroupingField} = @NaceFilter";

                string placeholders = string.Join(",", costTypes.Select((_, i) => $"@CostType{i}"));

                string query = $@"
                    SELECT 
                        co.value,
                        co.costtype_type,
                        y.year,
                        {naceGroupingField} as nace_group
                    FROM Costs co
                    JOIN Yearreports y ON co.yearreport_id = y.id
                    JOIN Companies c ON y.company_id = c.id
                    WHERE y.company_id != @CurrentCompanyId 
                    AND y.year = @Year
                    AND co.costtype_type IN ({placeholders})
                    {naceFilterCondition}";

                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@CurrentCompanyId", currentCompanyId),
                    new SqlParameter("@Year", year)
                };

                for (int i = 0; i < costTypes.Count; i++)
                {
                    parameters.Add(new SqlParameter($"@CostType{i}", costTypes[i]));
                }

                if (!string.IsNullOrEmpty(naceFilter))
                {
                    parameters.Add(new SqlParameter("@NaceFilter", naceFilter));
                }

                var results = DbHelper.ExecuteQuery(query, parameters.ToArray());

                foreach (var row in results)
                {
                    result.Add(new BenchmarkData
                    {
                        Indicator = "cost_" + row["costtype_type"].ToString(),
                        IndicatorType = "cost",
                        Waarde = Convert.ToDecimal(row["value"]),
                        Jaar = Convert.ToInt32(row["year"]),
                        NaceCode = row["nace_group"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen kosten benchmark data: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Haalt answers benchmark data op
        /// </summary>
        /// <param name="currentCompanyId">ID van het huidige bedrijf (om uit te sluiten)</param>
        /// <param name="year">Jaar waarvoor data opgehaald moet worden</param>
        /// <param name="naceFilter">Filter voor NACE-code (kan null zijn voor geen filter)</param>
        /// <param name="groupingLevel">Niveau van NACE-code groepering</param>
        /// <param name="questionIds">Lijst van vraag-IDs</param>
        /// <returns>Lijst van answers benchmark data</returns>
        private List<BenchmarkData> GetAnswersBenchmarkData(
            int currentCompanyId,
            int year,
            string naceFilter,
            NaceGroupingLevel groupingLevel,
            List<int> questionIds)
        {
            var result = new List<BenchmarkData>();

            try
            {
                string naceGroupingField = GetNaceGroupingField(groupingLevel);
                string naceFilterCondition = string.IsNullOrEmpty(naceFilter) 
                    ? "" 
                    : $"AND {naceGroupingField} = @NaceFilter";

                string placeholders = string.Join(",", questionIds.Select((_, i) => $"@QuestionId{i}"));

                string query = $@"
                    SELECT 
                        a.value,
                        a.question_id,
                        y.year,
                        {naceGroupingField} as nace_group
                    FROM Answers a
                    JOIN Yearreports y ON a.yearreport_id = y.id
                    JOIN Companies c ON y.company_id = c.id
                    JOIN Questions q ON a.question_id = q.id
                    WHERE y.company_id != @CurrentCompanyId 
                    AND y.year = @Year
                    AND q.type != 'info'
                    AND a.question_id IN ({placeholders})
                    {naceFilterCondition}";

                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@CurrentCompanyId", currentCompanyId),
                    new SqlParameter("@Year", year)
                };

                for (int i = 0; i < questionIds.Count; i++)
                {
                    parameters.Add(new SqlParameter($"@QuestionId{i}", questionIds[i]));
                }

                if (!string.IsNullOrEmpty(naceFilter))
                {
                    parameters.Add(new SqlParameter("@NaceFilter", naceFilter));
                }

                var results = DbHelper.ExecuteQuery(query, parameters.ToArray());

                foreach (var row in results)
                {
                    decimal waarde;
                    if (decimal.TryParse(row["value"].ToString(), out waarde))
                    {
                        result.Add(new BenchmarkData
                        {
                            Indicator = "question_" + row["question_id"].ToString(),
                            IndicatorType = "question",
                            Waarde = waarde,
                            Jaar = Convert.ToInt32(row["year"]),
                            NaceCode = row["nace_group"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij ophalen antwoorden benchmark data: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Krijgt het correcte veld voor NACE-code groepering op basis van het niveau
        /// </summary>
        /// <param name="groupingLevel">Niveau van NACE-code groepering</param>
        /// <returns>SQL expressie voor het groeperen van NACE-codes</returns>
        private string GetNaceGroupingField(NaceGroupingLevel groupingLevel)
        {
            switch (groupingLevel)
            {
                case NaceGroupingLevel.Niveau2Cijfers:
                    return "SUBSTRING(c.nacecode_code, 1, 2)";
                case NaceGroupingLevel.Niveau3Cijfers:
                    return "SUBSTRING(c.nacecode_code, 1, 3)";
                case NaceGroupingLevel.Niveau4of5Cijfers:
                    return "c.nacecode_code"; // Volledige code
                default:
                    return "c.nacecode_code";
            }
        }
    }
} 