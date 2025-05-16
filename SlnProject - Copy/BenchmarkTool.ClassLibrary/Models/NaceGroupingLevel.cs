namespace BenchmarkTool.ClassLibrary.Models
{
    /// <summary>
    /// Enumeration voor de verschillende niveaus van NACE-code groepering
    /// </summary>
    public enum NaceGroupingLevel
    {
        /// <summary>
        /// 2-cijferige NACE-code groepering (hoofdcategorie)
        /// </summary>
        Niveau2Cijfers = 2,

        /// <summary>
        /// 3-cijferige NACE-code groepering (subcategorie)
        /// </summary>
        Niveau3Cijfers = 3,

        /// <summary>
        /// 4 of 5-cijferige NACE-code groepering (gedetailleerde categorie)
        /// </summary>
        Niveau4of5Cijfers = 5
    }
} 