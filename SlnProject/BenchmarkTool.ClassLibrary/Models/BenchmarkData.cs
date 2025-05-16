using System.Collections.Generic;

namespace BenchmarkTool.ClassLibrary.Models
{
    /// <summary>
    /// Model voor benchmark data
    /// </summary>
    public class BenchmarkData
    {
        /// <summary>
        /// Gets or sets de indicator naam (fte, kosttype of vraag-id)
        /// </summary>
        public string Indicator { get; set; }

        /// <summary>
        /// Gets or sets het type indicator (fte, kosttype of vraag)
        /// </summary>
        public string IndicatorType { get; set; }

        /// <summary>
        /// Gets or sets de waarde van de indicator
        /// </summary>
        public decimal Waarde { get; set; }

        /// <summary>
        /// Gets or sets het jaar van de waarde
        /// </summary>
        public int Jaar { get; set; }

        /// <summary>
        /// Gets or sets de NACE-code op het gevraagde groeperingsniveau
        /// </summary>
        public string NaceCode { get; set; }
    }

    /// <summary>
    /// Model voor benchmark resultaten
    /// </summary>
    public class BenchmarkResultaat
    {
        /// <summary>
        /// Gets or sets de lijst van benchmark data
        /// </summary>
        public List<BenchmarkData> BenchmarkGegevens { get; set; } = new List<BenchmarkData>();

        /// <summary>
        /// Gets or sets de eigen waarden voor vergelijking
        /// </summary>
        public List<BenchmarkData> EigenGegevens { get; set; } = new List<BenchmarkData>();
    }
} 