using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Core.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace DesktopApplicationTemplate.UI.Services
{
    public class CsvService
    {
        private readonly CsvViewerViewModel _viewModel;
        private readonly ILoggingService? _logger;
        private int _index = 0;
        private bool _headerWritten;

        public CsvService(CsvViewerViewModel vm, ILoggingService? logger = null)
        {
            _viewModel = vm;
            _logger = logger;
        }

        public void EnsureColumnsForService(string serviceName)
        {
            if (!_viewModel.Configuration.Columns.Any(c => c.Name == serviceName))
            {
                _viewModel.Configuration.Columns.Add(new CsvColumnConfig { Name = serviceName, Service = serviceName });
                _logger?.Log($"Added CSV column for service {serviceName}", LogLevel.Debug);
            }
            string sent = $"{serviceName} Sent";
            if (!_viewModel.Configuration.Columns.Any(c => c.Name == sent))
            {
                _viewModel.Configuration.Columns.Add(new CsvColumnConfig { Name = sent, Service = serviceName });
                _logger?.Log($"Added CSV column for service {sent}", LogLevel.Debug);
            }
            _viewModel.Save();
        }

        public void RecordLog(string serviceName, string message)
        {
            EnsureColumnsForService(serviceName);
            EnsureHeader();
            var columns = _viewModel.Configuration.Columns.Select(_ => string.Empty).ToArray();
            bool sent = message.Contains("Sending", System.StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("Sent", System.StringComparison.OrdinalIgnoreCase);
            string column = sent ? $"{serviceName} Sent" : serviceName;
            int index = _viewModel.Configuration.Columns.ToList().FindIndex(c => c.Name == column);
            if (index >= 0)
                columns[index] = message.Replace(',', ' ');
            AppendRow(columns);
            _logger?.Log($"Recorded log for {serviceName}: {message}", LogLevel.Debug);
        }

        public void AppendRow(IEnumerable<string?> values)
        {
            string fileName = BuildFileName();
            var line = string.Join(',', values.Select(v => v ?? string.Empty));
            File.AppendAllText(fileName, line + System.Environment.NewLine, Encoding.UTF8);
            _logger?.Log($"Appended row to {fileName}", LogLevel.Debug);
        }

        private void EnsureHeader()
        {
            if (_headerWritten)
                return;
            string fileName = BuildFileName();
            if (!File.Exists(fileName) || new FileInfo(fileName).Length == 0)
            {
                var header = string.Join(',', _viewModel.Configuration.Columns.Select(c => c.Name));
                File.AppendAllText(fileName, header + System.Environment.NewLine, Encoding.UTF8);
                _logger?.Log($"Wrote CSV header to {fileName}", LogLevel.Debug);
            }
            _headerWritten = true;
        }

        private string BuildFileName()
        {
            string pattern = _viewModel.Configuration.FileNamePattern;
            string name = pattern.Replace("{index}", _index.ToString());
            if (!name.Contains("{index}"))
                _index++;
            return name;
        }
    }
}
