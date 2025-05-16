namespace BenchmarkTool.ClassLibrary.Models
{
    /// <summary>
    /// Representeert een NACE-code in het systeem, komt overeen met de Nacecodes tabel in de database.
    /// </summary>
    public class Nacecode
    {
        /// <summary>
        /// Unieke code voor de NACE-code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Beschrijving van de NACE-code in het Nederlands
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Beschrijving van de NACE-code in het Frans
        /// </summary>
        public string TextFr { get; set; }

        /// <summary>
        /// Beschrijving van de NACE-code in het Engels
        /// </summary>
        public string TextEn { get; set; }

        /// <summary>
        /// Code van de parent NACE-code, indien van toepassing
        /// </summary>
        public string ParentCode { get; set; }
    }
} 