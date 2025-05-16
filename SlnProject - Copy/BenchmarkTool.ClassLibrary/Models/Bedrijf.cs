namespace BenchmarkTool.ClassLibrary.Models
{
    /// <summary>
    /// Representeert een bedrijf in het systeem, komt overeen met de Companies tabel in de database.
    /// </summary>
    public class Bedrijf
    {
        /// <summary>
        /// Unieke identifier voor het bedrijf
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Naam van het bedrijf
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Contactpersoon binnen het bedrijf
        /// </summary>
        public string Contact { get; set; }

        /// <summary>
        /// Adres van het bedrijf
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Postcode
        /// </summary>
        public string Zip { get; set; }

        /// <summary>
        /// Stad/gemeente
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Land
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Telefoonnummer
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// E-mailadres
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// BTW-nummer
        /// </summary>
        public string Btw { get; set; }

        /// <summary>
        /// Gebruikersnaam voor login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Gehashed wachtwoord
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Datum van registratie
        /// </summary>
        public DateTime RegDate { get; set; }

        /// <summary>
        /// Datum van goedkeuring
        /// </summary>
        public DateTime? AcceptDate { get; set; }

        /// <summary>
        /// Datum van laatste wijziging
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// Status van het bedrijf (Pending, Active, Rejected)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Taalvoorkeur
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Logo van het bedrijf als binaire data
        /// </summary>
        public byte[] Logo { get; set; }

        /// <summary>
        /// NACE-code van het bedrijf
        /// </summary>
        public string NacecodeCode { get; set; }
    }
} 