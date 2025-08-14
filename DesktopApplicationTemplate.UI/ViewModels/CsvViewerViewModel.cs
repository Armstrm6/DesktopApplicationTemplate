using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        public CsvConfiguration Configuration { get; private set; } = new();
        public CsvColumnConfig? SelectedColumn { get; set; }

        public ICommand AddColumnCommand { get; }
        public ICommand RemoveColumnCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand DebugSaveCommand { get; }

        public event Action? RequestClose;

        public CsvViewerViewModel(string? configPath = null)
        {
            _configPath = configPath ?? "csv_config.json";
            Load();
            AddColumnCommand = new RelayCommand(() => Configuration.Columns.Add(new CsvColumnConfig()));
            RemoveColumnCommand = new RelayCommand(() => { if (SelectedColumn != null) Configuration.Columns.Remove(SelectedColumn); });
            SaveCommand = new RelayCommand(Save);
            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());
            DebugSaveCommand = new RelayCommand(DebugSave);
        }

        private void Load()
        {
            if (System.IO.File.Exists(_configPath))
            {
                var json = System.IO.File.ReadAllText(_configPath);
                Configuration = JsonSerializer.Deserialize<CsvConfiguration>(json) ?? new CsvConfiguration();
            }
        }

        public void Save()
        {
            if (Configuration is null)
                return;

            var snapshot = new CsvConfiguration
            {
                FileNamePattern = Configuration.FileNamePattern,
                Columns = new ObservableCollection<CsvColumnConfig>(Configuration.Columns.Select(c => new CsvColumnConfig
                {
                    Name = c.Name,
                    Service = c.Service,
                    Script = c.Script
                }))
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            try
            {
                var json = JsonSerializer.Serialize(snapshot, options);
                File.WriteAllText(_configPath, json);
            }
            catch (StackOverflowException ex)
            {
                var debugPath = Path.GetTempFileName();
                var debugOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                File.WriteAllText(debugPath, JsonSerializer.Serialize(snapshot, debugOptions));
                Console.Error.WriteLine($"Stack overflow saving CSV configuration. Snapshot: {debugPath}");
                Environment.FailFast("Stack overflow during CSV save", ex);
            }
        }

        private void DebugSave()
        {
            Configuration = new CsvConfiguration
            {
                FileNamePattern = "debug.csv",
                Columns = new ObservableCollection<CsvColumnConfig>
                {
                    new CsvColumnConfig { Name = "Test", Service = "Test", Script = string.Empty }
                }
            };
            Save();
        }

        // Uses OnPropertyChanged from ViewModelBase
    }
}
