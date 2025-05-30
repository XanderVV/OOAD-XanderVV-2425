using System;
using System.Collections.Generic;
using System.Data;
using BenchmarkTool.ClassLibrary.Authentication;
using BenchmarkTool.ClassLibrary.Models;
using Microsoft.Data.SqlClient;

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
            string query = $"SELECT * FROM {TableName} WHERE id = @Id";
            SqlParameter parameter = new SqlParameter("@Id", id);

            var results = DbHelper.ExecuteQuery(query, parameter);
            if (results.Count == 0)
            {
                return null;
            }

            return MapToBedrijf(results[0]);
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

            var results = DbHelper.ExecuteQuery(query, parameter);

            if (results.Count == 0)
            {
                return null;
            }

            return MapToBedrijf(results[0]);
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

            Bedrijf result = null;
            DbHelper.ExecuteReader(query, delegate(SqlDataReader reader)
            {
                if (reader.Read())
                {
                    result = MapToBedrijf(reader);
                }
            }, parameter);

            return result;
        }

        /// <summary>
        /// Haalt een bedrijf op basis van login en password hash
        /// </summary>
        /// <param name="login">Login van het bedrijf</param>
        /// <param name="passwordHash">Hash van het wachtwoord</param>
        /// <returns>Het gevonden bedrijf of null</returns>
        public Bedrijf GetBedrijfByLoginAndPasswordHash(string login, string passwordHash)
        {
            string query = $"SELECT * FROM {TableName} WHERE login = @Login AND password_hash = @PasswordHash";
            SqlParameter[] parameters = { new SqlParameter("@Login", login), new SqlParameter("@PasswordHash", passwordHash) };

            var results = DbHelper.ExecuteQuery(query, parameters);
            if (results.Count == 0)
            {
                return null;
            }

            return MapToBedrijf(results[0]);
        }

        /// <summary>
        /// Haalt alle bedrijven op
        /// </summary>
        /// <returns>Lijst van alle bedrijven</returns>
        public List<Bedrijf> GetAll()
        {
            return GetAll<Bedrijf>(TableName, reader => MapToBedrijf(reader));
        }

        /// <summary>
        /// Haalt alle bedrijven op met status 'Pending'
        /// </summary>
        /// <returns>Lijst van bedrijven met status 'Pending'</returns>
        public List<Bedrijf> GetPendingRegistrations()
        {
            string query = $"SELECT * FROM {TableName} WHERE status = @Status";
            SqlParameter parameter = new SqlParameter("@Status", "Pending");

            return DbHelper.ExecuteList<Bedrijf>(query, reader => MapToBedrijf(reader), parameter);
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
                    bedrijf.Id = Convert.ToInt32(DbHelper.ExecuteScalar(queryMaxId));
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

                return Convert.ToInt32(DbHelper.ExecuteScalar(query, parameters));
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
                new SqlParameter("@Language", ConvertToDBValue(bedrijf.Language ?? "Nederlands")),
                new SqlParameter("@Logo", SqlDbType.VarBinary) { Value = ConvertToDBValue(bedrijf.Logo) },
                new SqlParameter("@NacecodeCode", ConvertToDBValue(bedrijf.NacecodeCode)),
            };

            int rowsAffected = DbHelper.ExecuteNonQuery(query, parameters);
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

                int rowsAffected = DbHelper.ExecuteNonQuery(query, parameters);
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

            int rowsAffected = DbHelper.ExecuteNonQuery(query, parameters);
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

            object result = DbHelper.ExecuteScalar(query, parameter);
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

            int rowsAffected = DbHelper.ExecuteNonQuery(query, parameters);
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

                int rowsAffected = DbHelper.ExecuteNonQuery(query, parameters);
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

            int rowsAffected = DbHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Mapt een dictionary naar een Bedrijf-object
        /// </summary>
        /// <param name="row">Dictionary met de data</param>
        /// <returns>Gemapte Bedrijf-object</returns>
        private Bedrijf MapToBedrijf(Dictionary<string, object> row)
        {
            return new Bedrijf
            {
                Id = Convert.ToInt32(row["id"]),
                Login = row["login"]?.ToString(),
                Password = row["password"]?.ToString(),
                Name = row["name"]?.ToString(),
                Address = row["address"]?.ToString(),
                Zip = row["zip"]?.ToString(),
                City = row["city"]?.ToString(),
                Country = row["country"]?.ToString(),
                Btw = row["btw"]?.ToString(),
                Contact = row["contact"]?.ToString(),
                Email = row["email"]?.ToString(),
                Phone = row["phone"]?.ToString(),
                Status = row["status"]?.ToString(),
                NacecodeCode = row["nacecode_code"]?.ToString()
            };
        }

        #region Private Methods

        /// <summary>
        /// Converteert een SqlDataReader naar een Bedrijf object
        /// </summary>
        /// <param name="reader">De SqlDataReader met bedrijfsgegevens</param>
        /// <returns>Een Bedrijf object</returns>
        private Bedrijf MapToBedrijf(SqlDataReader reader)
        {
            try
            {
                return new Bedrijf
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Name = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : string.Empty,
                    Contact = reader["contact"] != DBNull.Value ? Convert.ToString(reader["contact"]) : null,
                    Address = reader["address"] != DBNull.Value ? Convert.ToString(reader["address"]) : null,
                    Zip = reader["zip"] != DBNull.Value ? Convert.ToString(reader["zip"]) : null,
                    City = reader["city"] != DBNull.Value ? Convert.ToString(reader["city"]) : null,
                    Country = reader["country"] != DBNull.Value ? Convert.ToString(reader["country"]) : null,
                    Phone = reader["phone"] != DBNull.Value ? Convert.ToString(reader["phone"]) : null,
                    Email = reader["email"] != DBNull.Value ? Convert.ToString(reader["email"]) : null,
                    Btw = reader["btw"] != DBNull.Value ? Convert.ToString(reader["btw"]) : null,
                    Login = reader["login"] != DBNull.Value ? Convert.ToString(reader["login"]) : string.Empty,
                    Password = reader["password"] != DBNull.Value ? Convert.ToString(reader["password"]) : null,
                    RegDate = reader["regdate"] != DBNull.Value ? Convert.ToDateTime(reader["regdate"]) : DateTime.MinValue,
                    AcceptDate = reader["acceptdate"] != DBNull.Value ? Convert.ToDateTime(reader["acceptdate"]) as DateTime? : null,
                    LastModified = reader["lastmodified"] != DBNull.Value ? Convert.ToDateTime(reader["lastmodified"]) as DateTime? : null,
                    Status = reader["status"] != DBNull.Value ? Convert.ToString(reader["status"]) : string.Empty,
                    Language = reader["language"] != DBNull.Value ? Convert.ToString(reader["language"]) : null,
                    Logo = reader["logo"] != DBNull.Value ? (byte[])reader["logo"] : null,
                    NacecodeCode = reader["nacecode_code"] != DBNull.Value ? Convert.ToString(reader["nacecode_code"]) : null
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