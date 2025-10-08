namespace DesktopApplicationTemplate.UI.Services
{
    public interface IFileDialogService
    {
        /// <summary>
        /// Opens a file selection dialog and returns the chosen file path or null if cancelled.
        /// </summary>
        string? OpenFile();

        /// <summary>
        /// Opens a folder selection dialog and returns the chosen directory path or null if cancelled.
        /// </summary>
        string? SelectFolder();

        /// <summary>
        /// Opens a save file dialog with a suggested file name and filter, returning the chosen file path or null if cancelled.
        /// </summary>
        /// <param name="suggestedName">The default file name presented to the user.</param>
        /// <param name="filter">The file filter string, e.g. "Text files (*.txt)|*.txt".</param>
        string? SaveFile(string suggestedName, string filter);
    }
}
