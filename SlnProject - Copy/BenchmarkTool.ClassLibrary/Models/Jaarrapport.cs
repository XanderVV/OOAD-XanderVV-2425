namespace BenchmarkTool.ClassLibrary.Models
{
    /// <summary>
    /// Representeert een jaarrapport in het systeem, komt overeen met de Yearreports tabel in de database.
    /// </summary>
    public class Jaarrapport
    {
        /// <summary>
        /// Unieke identifier voor het jaarrapport
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Jaar waarop het rapport betrekking heeft
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Aantal FTE (Full Time Equivalent) werknemers
        /// </summary>
        public decimal Fte { get; set; }

        /// <summary>
        /// ID van het bedrijf waartoe dit rapport behoort
        /// </summary>
        public int CompanyId { get; set; }
    }
} 