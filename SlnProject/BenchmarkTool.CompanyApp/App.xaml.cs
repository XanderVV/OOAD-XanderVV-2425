using System.Configuration;
using System.Windows;
using BenchmarkTool.ClassLibrary.Models;

namespace BenchmarkTool.CompanyApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Houdt het momenteel ingelogde bedrijf bij over de hele applicatie
        /// </summary>
        public static Bedrijf IngelogdBedrijf { get; set; }
    }
}
