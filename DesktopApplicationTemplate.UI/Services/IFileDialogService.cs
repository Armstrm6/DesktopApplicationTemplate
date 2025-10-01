namespace DesktopApplicationTemplate.UI.Services
{
    public interface IFileDialogService
    {
        /// <summary>
        /// Opens a file selection dialog and returns the chosen file path or null if cancelled.
        /// </summary>
        /// <param name="filter">Optional filter applied to the dialog.</param>
        /// <param name="title">Optional dialog title.</param>
        string? OpenFile(string? filter = null, string? title = null);

        /// <summary>
        /// Opens a folder selection dialog and returns the chosen directory path or null if cancelled.
        /// </summary>
        string? SelectFolder();
    }
}
