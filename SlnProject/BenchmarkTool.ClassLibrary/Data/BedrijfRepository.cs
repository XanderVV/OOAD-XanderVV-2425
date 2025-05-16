using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using BenchmarkTool.ClassLibrary.Models;
using BenchmarkTool.ClassLibrary.Authentication;

namespace BenchmarkTool.ClassLibrary.Data
{
    /// <summary>
    /// Repository voor het beheren van bedrijven in de database
    /// </summary>
    public class BedrijfRepository : DatabaseRepository
    {
        private const string TableName = "Companies";
        private const string IdColumn = "id";

        /// <summary>
        /// Haalt een bedrijf op basis van ID
        /// </summary>
        /// <param name="id">ID van het bedrijf</param>
        /// <returns>Het gevonden bedrijf of null</returns>
        public Bedrijf GetById(int id)
        {
            return GetById(TableName, IdColumn, id, MapToBedrijf);
        }

        /// <summary>
        /// Haalt een bedrijf op basis van gebruikersnaam
        /// </summary>
        /// <param name="username">Gebruikersnaam van het bedrijf</param>
        /// <returns>Het gevonden bedrijf of null</returns>
        public Bedrijf GetByUsername(string username)
        {
            string query = $"SELECT * FROM {TableName} WHERE login = @Username";
            SqlParameter parameter = new SqlParameter("@Username", username);

            DataTable result = _dbHelper.ExecuteDataTable(query, parameter);

            if (result.Rows.Count == 0)
            {
                return null;
            }

            return MapToBedrijf(result.Rows[0]);
        }

        /// <summary>
        /// Haalt een bedrijf op basis van login (gebruikersnaam)
        /// </summary>
        /// <param name="login">Login van het bedrijf</param>
        /// <returns>Het gevonden bedrijf of null</returns>
        public Bedrijf GetBedrijfByLogin(string login)
        {
            string query = $"SELECT * FROM {TableName} WHERE login = @Login";
            SqlParameter parameter = new SqlParameter("@Login", login);

            DataTable result = _dbHelper.ExecuteDataTable(query, parameter);

            if (result.Rows.Count == 0)
            {
                return null;
            }

            return MapToBedrijf(result.Rows[0]);
        }

        /// <summary>
        /// Haalt alle bedrijven op
        /// </summary>
        /// <returns>Lijst van alle bedrijven</returns>
        public List<Bedrijf> GetAll()
        {
            return GetAll(TableName, MapToBedrijf);
        }

        /// <summary>
        /// Haalt alle bedrijven op met status 'Pending'
        /// </summary>
        /// <returns>Lijst van bedrijven met status 'Pending'</returns>
        public List<Bedrijf> GetPendingRegistrations()
        {
            string query = $"SELECT * FROM {TableName} WHERE status = @Status";
            SqlParameter parameter = new SqlParameter("@Status", "Pending");

            return _dbHelper.ExecuteList(query, MapToBedrijf, parameter);
        }

        /// <summary>
        /// Voegt een nieuw bedrijf toe aan de database
        /// </summary>
        /// <param name="bedrijf">Het toe te voegen bedrijf</param>
        /// <returns>ID van het nieuwe bedrijf</returns>
        public int Create(Bedrijf bedrijf)
        {
            try
            {
                // Log voor debugging - volledige object details
                System.Diagnostics.Debug.WriteLine($"[CREATE] Bedrijf details:");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Id: {bedrijf.Id}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Name: {bedrijf.Name}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Contact: {bedrijf.Contact}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Email: {bedrijf.Email}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Address: {bedrijf.Address}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Zip: {bedrijf.Zip}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] City: {bedrijf.City}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Country: {bedrijf.Country}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Phone: {bedrijf.Phone}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Login: {bedrijf.Login}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Password: {(string.IsNullOrEmpty(bedrijf.Password) ? "Leeg" : "Gevuld")}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] RegDate: {bedrijf.RegDate}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Status: {bedrijf.Status}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Language: {bedrijf.Language}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] NacecodeCode: {bedrijf.NacecodeCode}");
                
                // Genereer een unieke ID als deze 0 is
                if (bedrijf.Id <= 0)
                {
                    // Haal de hoogste ID op uit de database en verhoog met 1
                    string queryMaxId = $"SELECT ISNULL(MAX(id), 0) + 1 FROM {TableName}";
                    bedrijf.Id = Convert.ToInt32(_dbHelper.ExecuteScalar(queryMaxId));
                    System.Diagnostics.Debug.WriteLine($"[CREATE] Gegenereerde nieuwe ID: {bedrijf.Id}");
                }
                
                string query = @"
                    INSERT INTO Companies (
                        id, name, contact, address, zip, city, country, phone,
                        email, btw, login, password, regdate, status, language, logo, nacecode_code
                    ) VALUES (
                        @Id, @Name, @Contact, @Address, @Zip, @City, @Country, @Phone,
                        @Email, @Btw, @Login, @Password, @RegDate, @Status, @Language, @Logo, @NacecodeCode
                    );
                    SELECT @Id;";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Id", bedrijf.Id),
                    new SqlParameter("@Name", bedrijf.Name),
                    new SqlParameter("@Contact", ConvertToDBValue(bedrijf.Contact)),
                    new SqlParameter("@Address", ConvertToDBValue(bedrijf.Address)),
                    new SqlParameter("@Zip", ConvertToDBValue(bedrijf.Zip)),
                    new SqlParameter("@City", ConvertToDBValue(bedrijf.City)),
                    new SqlParameter("@Country", ConvertToDBValue(bedrijf.Country)),
                    new SqlParameter("@Phone", ConvertToDBValue(bedrijf.Phone)),
                    new SqlParameter("@Email", ConvertToDBValue(bedrijf.Email)),
                    new SqlParameter("@Btw", ConvertToDBValue(bedrijf.Btw)),
                    new SqlParameter("@Login", bedrijf.Login),
                    new SqlParameter("@Password", ConvertToDBValue(bedrijf.Password)),
                    new SqlParameter("@RegDate", bedrijf.RegDate),
                    new SqlParameter("@Status", bedrijf.Status ?? "Pending"),
                    new SqlParameter("@Language", ConvertToDBValue(bedrijf.Language ?? "Nederlands")),
                    new SqlParameter("@Logo", SqlDbType.VarBinary) { Value = ConvertToDBValue(bedrijf.Logo) },
                    new SqlParameter("@NacecodeCode", ConvertToDBValue(bedrijf.NacecodeCode)),
                };

                // Log alle parameters
                foreach (var param in parameters)
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE] Parameter: {param.ParameterName} = {(param.Value == DBNull.Value ? "NULL" : param.Value.ToString())}");
                }

                return Convert.ToInt32(_dbHelper.ExecuteScalar(query, parameters));
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"[CREATE] Fout bij toevoegen bedrijf: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CREATE] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE] Inner exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"[CREATE] Inner stack trace: {ex.InnerException.StackTrace}");
                }
                throw; // Gooi de exception door zodat de UI deze kan afhandelen
            }
        }

        /// <summary>
        /// Werkt een bestaand bedrijf bij in de database
        /// </summary>
        /// <param name="bedrijf">Het bij te werken bedrijf</param>
        /// <returns>True als het bedrijf succesvol is bijgewerkt</returns>
        public bool Update(Bedrijf bedrijf)
        {
            string query = @"
                UPDATE Companies SET
                    name = @Name,
                    contact = @Contact,
                    address = @Address,
                    zip = @Zip,
                    city = @City,
                    country = @Country,
                    phone = @Phone,
                    email = @Email,
                    btw = @Btw,
                    login = @Login,
                    password = @Password,
                    acceptdate = @AcceptDate,
                    lastmodified = @LastModified,
                    status = @Status,
                    language = @Language,
                    logo = @Logo,
                    nacecode_code = @NacecodeCode
                WHERE id = @Id";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", bedrijf.Id),
                new SqlParameter("@Name", bedrijf.Name),
                new SqlParameter("@Contact", ConvertToDBValue(bedrijf.Contact)),
                new SqlParameter("@Address", ConvertToDBValue(bedrijf.Address)),
                new SqlParameter("@Zip", ConvertToDBValue(bedrijf.Zip)),
                new SqlParameter("@City", ConvertToDBValue(bedrijf.City)),
                new SqlParameter("@Country", ConvertToDBValue(bedrijf.Country)),
                new SqlParameter("@Phone", ConvertToDBValue(bedrijf.Phone)),
                new SqlParameter("@Email", ConvertToDBValue(bedrijf.Email)),
                new SqlParameter("@Btw", ConvertToDBValue(bedrijf.Btw)),
                new SqlParameter("@Login", bedrijf.Login),
                new SqlParameter("@Password", ConvertToDBValue(bedrijf.Password)),
                new SqlParameter("@AcceptDate", ConvertToDBValue(bedrijf.AcceptDate)),
                new SqlParameter("@LastModified", ConvertToDBValue(DateTime.Now)),
                new SqlParameter("@Status", bedrijf.Status),
                new SqlParameter("@Language", ConvertToDBValue(bedrijf.Language)),
                new SqlParameter("@Logo", SqlDbType.VarBinary) { Value = ConvertToDBValue(bedrijf.Logo) },
                new SqlParameter("@NacecodeCode", ConvertToDBValue(bedrijf.NacecodeCode)),
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Verwijdert een bedrijf uit de database
        /// </summary>
        /// <param name="id">ID van het te verwijderen bedrijf</param>
        /// <returns>True als het bedrijf succesvol is verwijderd</returns>
        public bool Delete(int id)
        {
            return DeleteById(TableName, IdColumn, id);
        }

        /// <summary>
        /// Keurt een registratie goed en stelt het wachtwoord in
        /// </summary>
        /// <param name="id">ID van het bedrijf</param>
        /// <param name="password">Wachtwoord (wordt gehashed opgeslagen)</param>
        /// <returns>True als de goedkeuring succesvol is</returns>
        public bool ApproveRegistration(int id, string password)
        {
            try
            {
                // Hash het wachtwoord voordat het wordt opgeslagen
                var wachtwoordHasher = new WachtwoordHasher();
                string hashedPassword = wachtwoordHasher.HashWachtwoord(password);
                
                string query = @"
                    UPDATE Companies SET
                        password = @Password,
                        status = 'Active',
                        acceptdate = @AcceptDate,
                        lastmodified = @LastModified
                    WHERE id = @Id";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@Password", hashedPassword),
                    new SqlParameter("@AcceptDate", DateTime.Now),
                    new SqlParameter("@LastModified", DateTime.Now),
                };

                int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij goedkeuren registratie: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw; // Gooi de exception door zodat de UI deze kan afhandelen
            }
        }

        /// <summary>
        /// Wijst een registratie af
        /// </summary>
        /// <param name="id">ID van het bedrijf</param>
        /// <returns>True als de afwijzing succesvol is</returns>
        public bool RejectRegistration(int id)
        {
            string query = @"
                UPDATE Companies SET
                    status = 'Rejected',
                    lastmodified = @LastModified
                WHERE id = @Id";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@LastModified", DateTime.Now),
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Haalt het logo van een bedrijf op
        /// </summary>
        /// <param name="id">ID van het bedrijf</param>
        /// <returns>Het logo als byte[] of null</returns>
        public byte[] GetLogo(int id)
        {
            string query = "SELECT logo FROM Companies WHERE id = @Id";
            SqlParameter parameter = new SqlParameter("@Id", id);

            object result = _dbHelper.ExecuteScalar(query, parameter);
            return result as byte[];
        }

        /// <summary>
        /// Werkt het logo van een bedrijf bij
        /// </summary>
        /// <param name="id">ID van het bedrijf</param>
        /// <param name="logo">Het nieuwe logo</param>
        /// <returns>True als het logo succesvol is bijgewerkt</returns>
        public bool UpdateLogo(int id, byte[] logo)
        {
            string query = @"
                UPDATE Companies SET
                    logo = @Logo,
                    lastmodified = @LastModified
                WHERE id = @Id";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@Logo", SqlDbType.VarBinary) { Value = logo ?? (object)DBNull.Value },
                new SqlParameter("@LastModified", DateTime.Now),
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Werkt het wachtwoord van een bedrijf bij
        /// </summary>
        /// <param name="id">ID van het bedrijf</param>
        /// <param name="password">Het nieuwe (gehashte) wachtwoord</param>
        /// <returns>True als het wachtwoord succesvol is bijgewerkt</returns>
        public bool UpdatePassword(int id, string password)
        {
            try
            {
                // Hash het wachtwoord voordat het wordt opgeslagen
                var wachtwoordHasher = new WachtwoordHasher();
                string hashedPassword = wachtwoordHasher.HashWachtwoord(password);
                
                string query = @"
                    UPDATE Companies SET
                        password = @Password,
                        lastmodified = @LastModified
                    WHERE id = @Id";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@Password", hashedPassword),
                    new SqlParameter("@LastModified", DateTime.Now),
                };

                int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij bijwerken van wachtwoord: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw; // Gooi de exception door zodat de UI deze kan afhandelen
            }
        }

        /// <summary>
        /// Alias voor Create methode - voegt een nieuw bedrijf toe aan de database
        /// </summary>
        /// <param name="bedrijf">Het toe te voegen bedrijf</param>
        /// <returns>ID van het nieuwe bedrijf</returns>
        public int Add(Bedrijf bedrijf)
        {
            return Create(bedrijf);
        }

        /// <summary>
        /// Voegt een nieuw bedrijf toe aan de database met expliciet wachtwoord
        /// </summary>
        /// <param name="bedrijf">Het toe te voegen bedrijf</param>
        /// <param name="password">Het wachtwoord voor het bedrijf</param>
        /// <returns>ID van het nieuwe bedrijf</returns>
        public int Add(Bedrijf bedrijf, string password)
        {
            try
            {
                // Log voor debugging
                System.Diagnostics.Debug.WriteLine($"Add bedrijf met wachtwoord: {bedrijf.Name}, Login: {bedrijf.Login}");
                
                // Hash het wachtwoord voordat het wordt opgeslagen
                var wachtwoordHasher = new WachtwoordHasher();
                string hashedPassword = wachtwoordHasher.HashWachtwoord(password);
                
                // Sla het gehashte wachtwoord op in het bedrijfsobject
                bedrijf.Password = hashedPassword;
                
                return Create(bedrijf);
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij toevoegen van bedrijf: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw; // Gooi de exception door zodat de UI deze kan afhandelen
            }
        }

        /// <summary>
        /// Verwijdert het logo van een bedrijf
        /// </summary>
        /// <param name="id">ID van het bedrijf</param>
        /// <returns>True als het logo succesvol is verwijderd</returns>
        public bool DeleteLogo(int id)
        {
            string query = @"
                UPDATE Companies SET
                    logo = NULL,
                    lastmodified = @LastModified
                WHERE id = @Id";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@LastModified", DateTime.Now),
            };

            int rowsAffected = _dbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        #region Private Methods

        /// <summary>
        /// Converteert een DataRow naar een Bedrijf object
        /// </summary>
        /// <param name="row">De DataRow met bedrijfsgegevens</param>
        /// <returns>Een Bedrijf object</returns>
        private Bedrijf MapToBedrijf(DataRow row)
        {
            try
            {
                return new Bedrijf
                {
                    Id = Convert.ToInt32(row["id"]),
                    Name = row["name"] != DBNull.Value ? Convert.ToString(row["name"]) : string.Empty,
                    Contact = row["contact"] != DBNull.Value ? Convert.ToString(row["contact"]) : null,
                    Address = row["address"] != DBNull.Value ? Convert.ToString(row["address"]) : null,
                    Zip = row["zip"] != DBNull.Value ? Convert.ToString(row["zip"]) : null,
                    City = row["city"] != DBNull.Value ? Convert.ToString(row["city"]) : null,
                    Country = row["country"] != DBNull.Value ? Convert.ToString(row["country"]) : null,
                    Phone = row["phone"] != DBNull.Value ? Convert.ToString(row["phone"]) : null,
                    Email = row["email"] != DBNull.Value ? Convert.ToString(row["email"]) : null,
                    Btw = row["btw"] != DBNull.Value ? Convert.ToString(row["btw"]) : null,
                    Login = row["login"] != DBNull.Value ? Convert.ToString(row["login"]) : string.Empty,
                    Password = row["password"] != DBNull.Value ? Convert.ToString(row["password"]) : null,
                    RegDate = row["regdate"] != DBNull.Value ? Convert.ToDateTime(row["regdate"]) : DateTime.MinValue,
                    AcceptDate = row["acceptdate"] != DBNull.Value ? Convert.ToDateTime(row["acceptdate"]) as DateTime? : null,
                    LastModified = row["lastmodified"] != DBNull.Value ? Convert.ToDateTime(row["lastmodified"]) as DateTime? : null,
                    Status = row["status"] != DBNull.Value ? Convert.ToString(row["status"]) : string.Empty,
                    Language = row["language"] != DBNull.Value ? Convert.ToString(row["language"]) : null,
                    Logo = row["logo"] != DBNull.Value ? (byte[])row["logo"] : null,
                    NacecodeCode = row["nacecode_code"] != DBNull.Value ? Convert.ToString(row["nacecode_code"]) : null
                };
            }
            catch (Exception ex)
            {
                // Log de fout
                System.Diagnostics.Debug.WriteLine($"Fout bij het mappen van bedrijf: {ex.Message}");
                throw new Exception("Er is een fout opgetreden bij het verwerken van bedrijfsgegevens.", ex);
            }
        }

        #endregion
    }
} 