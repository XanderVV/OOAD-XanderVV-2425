using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using BenchmarkTool.ClassLibrary.Models;

namespace BenchmarkTool.ClassLibrary.Data
{
    /// <summary>
    /// Repository voor het beheren van jaarrapporten in de database
    /// </summary>
    public class JaarrapportRepository : DatabaseRepository, IJaarrapportRepository
    {
        private const string TableName = "Yearreports";
        private const string IdColumn = "id";

        /// <summary>
        /// Haalt een jaarrapport op basis van ID
        /// </summary>
        /// <param name="id">ID van het jaarrapport</param>
        /// <returns>Het gevonden jaarrapport of null</returns>
        public Jaarrapport GetById(int id)
        {
            return GetById(TableName, IdColumn, id, MapToJaarrapport);
        }

        /// <summary>
        /// Haalt alle jaarrapporten op voor een specifiek bedrijf
        /// </summary>
        /// <param name="companyId">ID van het bedrijf</param>
        /// <returns>Lijst van jaarrapporten voor het bedrijf</returns>
        public List<Jaarrapport> GetByCompanyId(int companyId)
        {
            string query = $"SELECT * FROM {TableName} WHERE company_id = @CompanyId";
            SqlParameter parameter = new SqlParameter("@CompanyId", companyId);

            return _dbHelper.ExecuteList(query, MapToJaarrapport, parameter);
        }

        /// <summary>
        /// Haalt een jaarrapport op basis van jaar en bedrijfs-ID
        /// </summary>
        /// <param name="year">Jaar van het rapport</param>
        /// <param name="companyId">ID van het bedrijf</param>
        /// <returns>Het gevonden jaarrapport of null</returns>
        public Jaarrapport GetByYearAndCompany(int year, int companyId)
        {
            string query = $"SELECT * FROM {TableName} WHERE year = @Year AND company_id = @CompanyId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Year", year),
                new SqlParameter("@CompanyId", companyId)
            };

            DataTable result = _dbHelper.ExecuteDataTable(query, parameters);

            if (result.Rows.Count == 0)
            {
                return null;
            }

            return MapToJaarrapport(result.Rows[0]);
        }

        /// <summary>
        /// Voegt een nieuw jaarrapport toe aan de database
        /// </summary>
        /// <param name="jaarrapport">Het toe te voegen jaarrapport</param>
        /// <returns>ID van het nieuwe jaarrapport</returns>
        public int Create(Jaarrapport jaarrapport)
        {
            string query = @"
                INSERT INTO Yearreports (
                    year, fte, company_id
                ) VALUES (
                    @Year, @Fte, @CompanyId
                );
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Year", jaarrapport.Year),
                new SqlParameter("@Fte", jaarrapport.Fte),
                new SqlParameter("@CompanyId", jaarrapport.CompanyId)
            };

            return Convert.ToInt32(_dbHelper.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Werkt een bestaand jaarrapport bij in de database
        /// </summary>
        /// <param name="jaarrapport">Het bij te werken jaarrapport</param>
        /// <returns>True als het jaarrapport succesvol is bijgewerkt</returns>
        public bool Update(Jaarrapport jaarrapport)
        {
            string query = @"
                UPDATE Yearreports SET
                    year = @Year,
                    fte = @Fte,
                    company_id = @CompanyId
                WHERE id = @Id";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", jaarrapport.Id),
                new SqlParameter("@Year", jaarrapport.Year),
                new SqlParameter("@Fte", jaarrapport.Fte),
                new SqlParameter("@CompanyId", jaarrapport.CompanyId)
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Verwijdert een jaarrapport uit de database
        /// </summary>
        /// <param name="id">ID van het te verwijderen jaarrapport</param>
        /// <returns>True als het jaarrapport succesvol is verwijderd</returns>
        public bool Delete(int id)
        {
            return DeleteById(TableName, IdColumn, id);
        }

        /// <summary>
        /// Haalt alle kosten op voor een specifiek jaarrapport
        /// </summary>
        /// <param name="yearreportId">ID van het jaarrapport</param>
        /// <returns>Lijst van kosten voor het jaarrapport</returns>
        public List<Kost> GetCosts(int yearreportId)
        {
            string query = "SELECT * FROM Costs WHERE yearreport_id = @YearreportId";
            SqlParameter parameter = new SqlParameter("@YearreportId", yearreportId);

            return _dbHelper.ExecuteList(query, MapToKost, parameter);
        }

        /// <summary>
        /// Haalt alle antwoorden op voor een specifiek jaarrapport
        /// </summary>
        /// <param name="yearreportId">ID van het jaarrapport</param>
        /// <returns>Lijst van antwoorden voor het jaarrapport</returns>
        public List<Antwoord> GetAnswers(int yearreportId)
        {
            string query = "SELECT * FROM Answers WHERE yearreport_id = @YearreportId";
            SqlParameter parameter = new SqlParameter("@YearreportId", yearreportId);

            return _dbHelper.ExecuteList(query, MapToAntwoord, parameter);
        }

        /// <summary>
        /// Slaat kosten op voor een jaarrapport
        /// </summary>
        /// <param name="yearreportId">ID van het jaarrapport</param>
        /// <param name="costs">Lijst van kosten</param>
        /// <returns>Aantal succesvol opgeslagen kosten</returns>
        public int SaveCosts(int yearreportId, List<Kost> costs)
        {
            // Eerst bestaande kosten verwijderen
            string deleteQuery = "DELETE FROM Costs WHERE yearreport_id = @YearreportId";
            SqlParameter deleteParameter = new SqlParameter("@YearreportId", yearreportId);
            _dbHelper.ExecuteNonQuery(deleteQuery, deleteParameter);

            // Nieuwe kosten toevoegen
            int successCount = 0;
            foreach (var cost in costs)
            {
                string insertQuery = @"
                    INSERT INTO Costs (
                        value, costtype_type, category_nr, yearreport_id
                    ) VALUES (
                        @Value, @CosttypeType, @CategoryNr, @YearreportId
                    )";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Value", cost.Value),
                    new SqlParameter("@CosttypeType", cost.CosttypeType),
                    new SqlParameter("@CategoryNr", cost.CategoryNr),
                    new SqlParameter("@YearreportId", yearreportId)
                };

                try
                {
                    _dbHelper.ExecuteNonQuery(insertQuery, parameters);
                    successCount++;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Fout bij opslaan kost: {ex.Message}");
                }
            }

            return successCount;
        }

        /// <summary>
        /// Slaat antwoorden op voor een jaarrapport
        /// </summary>
        /// <param name="yearreportId">ID van het jaarrapport</param>
        /// <param name="answers">Lijst van antwoorden</param>
        /// <returns>Aantal succesvol opgeslagen antwoorden</returns>
        public int SaveAnswers(int yearreportId, List<Antwoord> answers)
        {
            // Eerst bestaande antwoorden verwijderen
            string deleteQuery = "DELETE FROM Answers WHERE yearreport_id = @YearreportId";
            SqlParameter deleteParameter = new SqlParameter("@YearreportId", yearreportId);
            _dbHelper.ExecuteNonQuery(deleteQuery, deleteParameter);

            // Nieuwe antwoorden toevoegen
            int successCount = 0;
            foreach (var answer in answers)
            {
                string insertQuery = @"
                    INSERT INTO Answers (
                        value, question_id, yearreport_id
                    ) VALUES (
                        @Value, @QuestionId, @YearreportId
                    )";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Value", ConvertToDBValue(answer.Value)),
                    new SqlParameter("@QuestionId", answer.QuestionId),
                    new SqlParameter("@YearreportId", yearreportId)
                };

                try
                {
                    _dbHelper.ExecuteNonQuery(insertQuery, parameters);
                    successCount++;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Fout bij opslaan antwoord: {ex.Message}");
                }
            }

            return successCount;
        }

        /// <summary>
        /// Haalt alle vragen op die niet van het type 'info' zijn
        /// </summary>
        /// <returns>Lijst van vragen</returns>
        public List<Vraag> GetQuestionsNotInfo()
        {
            string query = "SELECT * FROM Questions WHERE type != 'info'";
            return _dbHelper.ExecuteList(query, MapToVraag);
        }

        #region Private Methods

        /// <summary>
        /// Converteert een DataRow naar een Jaarrapport object
        /// </summary>
        /// <param name="row">De DataRow met jaarrapportgegevens</param>
        /// <returns>Een Jaarrapport object</returns>
        private Jaarrapport MapToJaarrapport(DataRow row)
        {
            return new Jaarrapport
            {
                Id = Convert.ToInt32(row["id"]),
                Year = Convert.ToInt32(row["year"]),
                Fte = Convert.ToDecimal(row["fte"]),
                CompanyId = Convert.ToInt32(row["company_id"])
            };
        }

        /// <summary>
        /// Converteert een DataRow naar een Kost object
        /// </summary>
        /// <param name="row">De DataRow met kostgegevens</param>
        /// <returns>Een Kost object</returns>
        private Kost MapToKost(DataRow row)
        {
            return new Kost
            {
                Id = Convert.ToInt32(row["id"]),
                Value = Convert.ToDecimal(row["value"]),
                CosttypeType = row["costtype_type"].ToString(),
                CategoryNr = Convert.ToInt32(row["category_nr"]),
                YearreportId = Convert.ToInt32(row["yearreport_id"])
            };
        }

        /// <summary>
        /// Converteert een DataRow naar een Antwoord object
        /// </summary>
        /// <param name="row">De DataRow met antwoordgegevens</param>
        /// <returns>Een Antwoord object</returns>
        private Antwoord MapToAntwoord(DataRow row)
        {
            return new Antwoord
            {
                Id = Convert.ToInt32(row["id"]),
                Value = ConvertFromDBValue<string>(row["value"]),
                QuestionId = Convert.ToInt32(row["question_id"]),
                YearreportId = Convert.ToInt32(row["yearreport_id"])
            };
        }

        /// <summary>
        /// Converteert een DataRow naar een Vraag object
        /// </summary>
        /// <param name="row">De DataRow met vraaggegevens</param>
        /// <returns>Een Vraag object</returns>
        private Vraag MapToVraag(DataRow row)
        {
            return new Vraag
            {
                Id = Convert.ToInt32(row["id"]),
                Text = row["text"].ToString(),
                TextFr = ConvertFromDBValue<string>(row["textFr"]),
                TextEn = ConvertFromDBValue<string>(row["textEn"]),
                Tooltip = ConvertFromDBValue<string>(row["tooltip"]),
                TooltipFr = ConvertFromDBValue<string>(row["tooltipFr"]),
                TooltipEn = ConvertFromDBValue<string>(row["tooltipEn"]),
                Type = row["type"].ToString(),
                Values = ConvertFromDBValue<string>(row["values"]),
                ValuesFr = ConvertFromDBValue<string>(row["valuesFr"]),
                ValuesEn = ConvertFromDBValue<string>(row["valuesEn"]),
                MaxValue = ConvertFromDBValue<decimal?>(row["maxvalue"]),
                CategoryNr = Convert.ToInt32(row["category_nr"])
            };
        }

        #endregion
    }
} 