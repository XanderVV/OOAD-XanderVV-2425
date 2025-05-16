namespace BenchmarkTool.ClassLibrary.Models
{
    /// <summary>
    /// Representeert een categorie in het systeem, komt overeen met de Categories tabel in de database.
    /// </summary>
    public class Categorie
    {
        /// <summary>
        /// Unieke identifier voor de categorie
        /// </summary>
        public int Nr { get; set; }

        /// <summary>
        /// Naam van de categorie in het Nederlands
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Naam van de categorie in het Frans
        /// </summary>
        public string TextFr { get; set; }

        /// <summary>
        /// Naam van de categorie in het Engels
        /// </summary>
        public string TextEn { get; set; }

        /// <summary>
        /// Tooltip/hulptekst in het Nederlands
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Tooltip/hulptekst in het Frans
        /// </summary>
        public string TooltipFr { get; set; }

        /// <summary>
        /// Tooltip/hulptekst in het Engels
        /// </summary>
        public string TooltipEn { get; set; }

        /// <summary>
        /// Lijst van relevante kosttypes voor deze categorie
        /// </summary>
        public string RelevantCostTypes { get; set; }

        /// <summary>
        /// Identifier van de parent categorie, indien van toepassing
        /// </summary>
        public int? ParentNr { get; set; }
    }
} 