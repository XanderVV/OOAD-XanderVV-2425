namespace BenchmarkTool.ClassLibrary.Models
{
    /// <summary>
    /// Representeert een antwoord op een vraag in het systeem, komt overeen met de Answers tabel in de database.
    /// </summary>
    public class Antwoord
    {
        /// <summary>
        /// Unieke identifier voor het antwoord
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Waarde van het antwoord
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// ID van de vraag waartoe dit antwoord behoort
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// ID van het jaarrapport waartoe dit antwoord behoort
        /// </summary>
        public int YearreportId { get; set; }
    }
} 