using DesktopApplicationTemplate.UI.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DesktopApplicationTemplate.Core.Services;


namespace DesktopApplicationTemplate.UI.Services
{
    public class CsvService
    {
        private readonly CsvViewerViewModel _viewModel;
        private int _index = 0;
        private bool _headerWritten;
        private string? _currentFile;

        public CsvService(CsvViewerViewModel vm)
        {
            _viewModel = vm;
        }

        public void EnsureColumnsForService(string serviceName)
        {
            if (!_viewModel.Configuration.Columns.Any(c => c.Name == serviceName))
            {
                _viewModel.Configuration.Columns.Add(new CsvColumnConfig { Name = serviceName, Service = serviceName });
            }
            string sent = $"{serviceName} Sent";
            if (!_viewModel.Configuration.Columns.Any(c => c.Name == sent))
            {
                _viewModel.Configuration.Columns.Add(new CsvColumnConfig { Name = sent, Service = serviceName });
            }
            _viewModel.Save();
        }

        public void RemoveColumnsForService(string serviceName)
        {
            var sent = $"{serviceName} Sent";
            var toRemove = _viewModel.Configuration.Columns
                .Where(c => c.Name == serviceName || c.Name == sent)
                .ToList();
            foreach (var col in toRemove)
            {
                _viewModel.Configuration.Columns.Remove(col);
            }

            if (toRemove.Count > 0)
            {
                _viewModel.Save();
                _index++;
                _headerWritten = false;
            }
        }

        public void RecordLog(string serviceName, string message)
        {
            EnsureColumnsForService(serviceName);
            var fileName = BuildFileName(serviceName, message);
            EnsureHeader(fileName);
            var columns = _viewModel.Configuration.Columns.Select(_ => string.Empty).ToArray();
            bool sent = message.Contains("Sending", System.StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("Sent", System.StringComparison.OrdinalIgnoreCase);
            string column = sent ? $"{serviceName} Sent" : serviceName;
            int index = _viewModel.Configuration.Columns.ToList().FindIndex(c => c.Name == column);
            if (index >= 0)
                columns[index] = message.Replace(',', ' ');
            AppendRow(columns, fileName);
        }

        public void AppendRow(IEnumerable<string?> values, string? fileName = null, string serviceName = "", string message = "")
        {
            fileName ??= BuildFileName(serviceName, message);
            var dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            var line = string.Join(',', values.Select(v => v ?? string.Empty));
            File.AppendAllText(fileName, line + System.Environment.NewLine, Encoding.UTF8);
        }

        private void EnsureHeader(string fileName)
        {
            if (_headerWritten && _currentFile == fileName)
                return;
            if (_currentFile != fileName)
            {
                _currentFile = fileName;
                _headerWritten = false;
            }
            if (!_headerWritten)
            {
                if (!File.Exists(fileName) || new FileInfo(fileName).Length == 0)
                {
                    var header = string.Join(',', _viewModel.Configuration.Columns.Select(c => c.Name));
                    File.AppendAllText(fileName, header + System.Environment.NewLine, Encoding.UTF8);
                }
                _headerWritten = true;
            }
        }

        private string BuildFileName(string serviceName, string message)
        {
            string pattern = _viewModel.Configuration.FileNamePattern;
            string sanitizedService = Sanitize(serviceName);
            string sanitizedMessage = Sanitize(message);
            pattern = pattern
                .Replace("{service}", sanitizedService)
                .Replace("{message}", sanitizedMessage)
                .Replace("{ServiceName}", sanitizedService)
                .Replace("{ServiceMessage}", sanitizedMessage);
            string name = pattern.Replace("{index}", _index.ToString());
            if (pattern.Contains("{index}"))
                _index++;
            return name;
        }

        private static string Sanitize(string value)
        {
            foreach (var c in Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()))
            {
                value = value.Replace(c, '_');
            }
            return value;
        }

    }
}
