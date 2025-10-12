using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;

namespace DesktopApplicationTemplate.UI.Views.Shared
{
    /// <summary>
    /// Interaction logic for ServiceLogView.xaml
    /// </summary>
    public partial class ServiceLogView : UserControl
    {
        private const string ExportDialogFilter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*";
        private const string ExportTimestampFormat = "yyyyMMdd_HHmmss";
        private readonly AsyncRelayCommand _exportLogCommand;
        private bool _isExporting;

        public ICommand ExportLogCommand => _exportLogCommand;

        public ServiceLogView()
        {
            InitializeComponent();
            _exportLogCommand = new AsyncRelayCommand(ExportLogAsync, () => !_isExporting);
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

        private async Task ExportLogAsync()
        {
            if (DataContext is not ServiceLogViewModel viewModel)
            {
                return;
            }

            if (viewModel.IsAggregated && Window.GetWindow(this) is MainView mainView)
            {
                await ExecuteExportAsync(mainView.ExportAllLogsAsync).ConfigureAwait(false);
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

            await ExecuteExportAsync(() => viewModel.ExportLogsAsync(selectedPath)).ConfigureAwait(false);
        }

        private async Task ExecuteExportAsync(Func<Task> exportAction)
        {
            if (exportAction is null)
            {
                return;
            }

            if (_isExporting)
            {
                return;
            }

            _isExporting = true;
            _exportLogCommand.RaiseCanExecuteChanged();

            try
            {
                await exportAction().ConfigureAwait(false);
            }
            finally
            {
                _isExporting = false;
                _exportLogCommand.RaiseCanExecuteChanged();
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
