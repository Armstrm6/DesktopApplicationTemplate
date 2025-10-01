using Microsoft.Win32;
using System.IO;

namespace DesktopApplicationTemplate.UI.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFile(string? filter = null, string? title = null)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = string.IsNullOrWhiteSpace(filter) ? "All Files (*.*)|*.*" : filter,
                Title = string.IsNullOrWhiteSpace(title) ? "Select File" : title
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? SelectFolder()
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = false,
                FileName = "Select Folder"
            };
            return dialog.ShowDialog() == true ? Path.GetDirectoryName(dialog.FileName) : null;
        }
    }
}
