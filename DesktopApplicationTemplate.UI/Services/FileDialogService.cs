using Microsoft.Win32;
using System.IO;

namespace DesktopApplicationTemplate.UI.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? SelectFolder()
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection"
            };
            return dialog.ShowDialog() == true
                ? Path.GetDirectoryName(dialog.FileName)
                : null;
        }
    }
}
