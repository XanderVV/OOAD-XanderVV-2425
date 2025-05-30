using System.Collections.Generic;

namespace BenchmarkTool.ClassLibrary.Models
{
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