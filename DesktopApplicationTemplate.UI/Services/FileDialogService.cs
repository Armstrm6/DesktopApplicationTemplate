using Microsoft.Win32;

namespace DesktopApplicationTemplate.UI.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFile()
        {
            var dialog = new OpenFileDialog();
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
