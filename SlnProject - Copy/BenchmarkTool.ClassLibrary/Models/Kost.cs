namespace BenchmarkTool.ClassLibrary.Models
{
    /// <summary>
    /// Representeert een kostenpost in het systeem, komt overeen met de Costs tabel in de database.
    /// </summary>
    public class Kost
    {
        /// <summary>
        /// Unieke identifier voor de kostenpost
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Waarde van de kost
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Type van de kostenpost
        /// </summary>
        public string CosttypeType { get; set; }

        /// <summary>
        /// Categorie nummer waartoe de kostenpost behoort
        /// </summary>
        public int CategoryNr { get; set; }

        /// <summary>
        /// ID van het jaarrapport waartoe deze kostenpost behoort
        /// </summary>
        public int YearreportId { get; set; }
    }
} 