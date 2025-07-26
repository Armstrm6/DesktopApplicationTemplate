using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class CsvColumnConfig
    {
        public string Name { get; set; } = "Column";
        public string Service { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
    }

    public class CsvConfiguration
    {
        public string FileNamePattern { get; set; } = "output_{index}.csv";
        public ObservableCollection<CsvColumnConfig> Columns { get; set; } = new();
    }

    public class CsvViewerViewModel : ViewModelBase
    {
        private readonly string _configPath;
        private readonly DesktopApplicationTemplate.UI.Services.ILoggingService? _logger;

        public CsvConfiguration Configuration { get; private set; } = new();
        public CsvColumnConfig? SelectedColumn { get; set; }

        public ICommand AddColumnCommand { get; }
        public ICommand RemoveColumnCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action? RequestClose;

        public CsvViewerViewModel(string? configPath = null, DesktopApplicationTemplate.UI.Services.ILoggingService? logger = null)
        {
            _configPath = configPath ?? "csv_config.json";
            _logger = logger;
            Load();
            AddColumnCommand = new RelayCommand(() => Configuration.Columns.Add(new CsvColumnConfig()));
            RemoveColumnCommand = new RelayCommand(() => { if (SelectedColumn != null) Configuration.Columns.Remove(SelectedColumn); });
            SaveCommand = new RelayCommand(Save);
            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());
        }

        private void Load()
        {
            if (System.IO.File.Exists(_configPath))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(_configPath);
                    if (!string.IsNullOrWhiteSpace(json))
                        Configuration = JsonSerializer.Deserialize<CsvConfiguration>(json) ?? new CsvConfiguration();
                }
                catch (Exception ex)
                {
                    _logger?.Log($"Failed to load CSV config: {ex.Message}", DesktopApplicationTemplate.UI.Services.LogLevel.Error);
                }
            }
            else
            {
                _logger?.Log($"CSV config {_configPath} not found, using defaults", DesktopApplicationTemplate.UI.Services.LogLevel.Debug);
            }
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(Configuration, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_configPath, json);
            _logger?.Log($"CSV configuration saved to {_configPath}", DesktopApplicationTemplate.UI.Services.LogLevel.Debug);
        }

        // Uses OnPropertyChanged from ViewModelBase
    }
}
