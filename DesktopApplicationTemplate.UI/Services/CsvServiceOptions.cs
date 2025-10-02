namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Configuration options for CSV creator services.
    /// </summary>
    public class CsvServiceOptions
    {
        /// <summary>
        /// Output directory for generated CSV files.
        /// </summary>
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>
        /// Delimiter used between values.
        /// </summary>
        public string Delimiter { get; set; } = ",";

        /// <summary>
        /// Whether to include a header row in generated files.
        /// </summary>
        public bool IncludeHeaders { get; set; } = true;
    }
}
