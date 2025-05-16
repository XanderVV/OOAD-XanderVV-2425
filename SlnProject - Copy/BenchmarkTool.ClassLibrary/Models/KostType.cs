namespace BenchmarkTool.ClassLibrary.Models
{
    /// <summary>
    /// Representeert een type kostenpost in het systeem, komt overeen met de Costtypes tabel in de database.
    /// </summary>
    public class KostType
    {
        /// <summary>
        /// Unieke identifier voor het kostentype
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Beschrijving van het kostentype in het Nederlands
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Beschrijving van het kostentype in het Frans
        /// </summary>
        public string TextFr { get; set; }

        /// <summary>
        /// Beschrijving van het kostentype in het Engels
        /// </summary>
        public string TextEn { get; set; }
    }
} 