using System;
using System.Collections.ObjectModel;
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
#if DEBUG
        public ICommand DebugSaveCommand { get; }
#endif

        public event Action? RequestClose;

        public CsvViewerViewModel(string? configPath = null)
        {
            _configPath = configPath ?? "csv_config.json";
            Load();
            AddColumnCommand = new RelayCommand(() => Configuration.Columns.Add(new CsvColumnConfig()));
            RemoveColumnCommand = new RelayCommand(() => { if (SelectedColumn != null) Configuration.Columns.Remove(SelectedColumn); });
            SaveCommand = new RelayCommand(Save);
            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());
#if DEBUG
            DebugSaveCommand = new RelayCommand(() => Save());
#endif
        }

        private void Load()
        {
            if (System.IO.File.Exists(_configPath))
            {
                var json = System.IO.File.ReadAllText(_configPath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    Configuration = JsonSerializer.Deserialize<CsvConfiguration>(json) ?? new CsvConfiguration();
                }
            }
        }

        public void Save()
        {
            if (Configuration is null)
                return;

            try
            {
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

                var json = JsonSerializer.Serialize(snapshot, options);
                System.IO.File.WriteAllText(_configPath, json);
            }
            catch (StackOverflowException)
            {
                var dumpOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                var dump = JsonSerializer.Serialize(Configuration, dumpOptions);
                var temp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "csv_config_dump.json");
                System.IO.File.WriteAllText(temp, dump);
                Environment.FailFast($"Stack overflow while saving CSV configuration. Dump written to {temp}");
            }
        }

        // Uses OnPropertyChanged from ViewModelBase
    }
}
