using Microsoft.Win32;
using System;
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
                ValidateNames = false,
                FileName = "Select Folder"
            };
            return dialog.ShowDialog() == true ? Path.GetDirectoryName(dialog.FileName) : null;
        }

        public string? SaveFile(string suggestedName, string filter)
        {
            ArgumentNullException.ThrowIfNull(suggestedName);
            ArgumentNullException.ThrowIfNull(filter);

            var dialog = new SaveFileDialog
            {
                FileName = suggestedName,
                Filter = filter,
                AddExtension = true,
                OverwritePrompt = true
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
