namespace BenchmarkTool.ClassLibrary.Models
{
    /// <summary>
    /// Representeert een vraag in het systeem, komt overeen met de Questions tabel in de database.
    /// </summary>
    public class Vraag
    {
        /// <summary>
        /// Unieke identifier voor de vraag
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Vraag tekst in het Nederlands
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Vraag tekst in het Frans
        /// </summary>
        public string TextFr { get; set; }

        /// <summary>
        /// Vraag tekst in het Engels
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
        /// Type van de vraag (bijv. "info", "numeric", "text", enz.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Mogelijke waarden voor de vraag (indien van toepassing)
        /// </summary>
        public string Values { get; set; }

        /// <summary>
        /// Mogelijke waarden voor de vraag in het Frans
        /// </summary>
        public string ValuesFr { get; set; }

        /// <summary>
        /// Mogelijke waarden voor de vraag in het Engels
        /// </summary>
        public string ValuesEn { get; set; }

        /// <summary>
        /// Maximaal toegestane waarde (indien van toepassing)
        /// </summary>
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// Categorie nummer waartoe deze vraag behoort
        /// </summary>
        public int CategoryNr { get; set; }
    }
} 