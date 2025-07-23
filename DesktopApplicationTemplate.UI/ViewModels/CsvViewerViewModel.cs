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
        private const string ConfigPath = "csv_config.json";

        public CsvConfiguration Configuration { get; private set; } = new();
        public CsvColumnConfig? SelectedColumn { get; set; }

        public ICommand AddColumnCommand { get; }
        public ICommand RemoveColumnCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action? RequestClose;

        public CsvViewerViewModel()
        {
            Load();
            AddColumnCommand = new RelayCommand(() => Configuration.Columns.Add(new CsvColumnConfig()));
            RemoveColumnCommand = new RelayCommand(() => { if (SelectedColumn != null) Configuration.Columns.Remove(SelectedColumn); });
            SaveCommand = new RelayCommand(Save);
            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());
        }

        private void Load()
        {
            if (System.IO.File.Exists(ConfigPath))
            {
                var json = System.IO.File.ReadAllText(ConfigPath);
                Configuration = JsonSerializer.Deserialize<CsvConfiguration>(json) ?? new CsvConfiguration();
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(Configuration, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(ConfigPath, json);
        }

        // Uses OnPropertyChanged from ViewModelBase
    }
}
