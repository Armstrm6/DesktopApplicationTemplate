using Microsoft.Win32;

namespace DesktopApplicationTemplate.UI.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
