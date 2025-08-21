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
    }
}
