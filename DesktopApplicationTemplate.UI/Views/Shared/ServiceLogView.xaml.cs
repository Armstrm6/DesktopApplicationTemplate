using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views.Shared
{
    /// <summary>
    /// Interaction logic for ServiceLogView.xaml
    /// </summary>
    public partial class ServiceLogView : UserControl
    {
        private const string ExportDialogFilter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*";
        private const string ExportTimestampFormat = "yyyyMMdd_HHmmss";

        public ServiceLogView()
        {
            InitializeComponent();
        }

        private void CopyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LogListBox?.SelectedItems?.Count > 0;
        }

        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (LogListBox?.SelectedItems is null || LogListBox.SelectedItems.Count == 0)
            {
                return;
            }

            var lines = LogListBox.SelectedItems
                .OfType<LogEntry>()
                .Select(entry => entry.Message)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            if (lines.Length == 0)
            {
                return;
            }

            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine, lines));
            }
            catch (Exception)
            {
                // Clipboard.SetText can throw if the clipboard is unavailable; ignore and leave command silent.
            }
        }

        private async void ExportLog_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ServiceLogViewModel viewModel)
            {
                return;
            }

            var timestamp = DateTime.Now.ToString(ExportTimestampFormat);
            var sanitizedServiceName = SanitizeFileName(viewModel.Type.ToString());
            var suggestedFileName = $"{sanitizedServiceName}_{timestamp}.log";

            var selectedPath = App.FileDialogService.SaveFile(suggestedFileName, ExportDialogFilter);
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return;
            }

            if (sender is Button button)
            {
                button.IsEnabled = false;
                try
                {
                    await viewModel.ExportLogsAsync(selectedPath);
                }
                finally
                {
                    button.IsEnabled = true;
                }
            }
            else
            {
                await viewModel.ExportLogsAsync(selectedPath);
            }
        }

        private static string SanitizeFileName(string name)
        {
            var invalidCharacters = Path.GetInvalidFileNameChars();
            var sanitized = new string(name.Select(ch => invalidCharacters.Contains(ch) ? '_' : ch).ToArray());
            return string.IsNullOrWhiteSpace(sanitized) ? "ServiceLog" : sanitized;
        }
    }
}
