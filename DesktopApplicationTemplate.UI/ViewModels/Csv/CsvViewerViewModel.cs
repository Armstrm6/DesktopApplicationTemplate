using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels.Csv
{
    public class CsvViewerViewModel : ViewModelBase
    {
        private readonly string _configPath;
        private readonly IFileDialogService _fileDialog;
        private readonly IMessageRoutingService _routingService;
        private readonly ObservableCollection<string> _attributeSuggestions = new();
        private readonly ObservableCollection<string> _validationMessages = new();
        private readonly HashSet<string> _suggestionSet = new(StringComparer.OrdinalIgnoreCase);
        private ObservableCollection<CsvColumnDefinition>? _observableColumns;
        private CsvColumnDefinition? _selectedColumn;

        public CsvConfiguration Configuration { get; private set; } = CreateDefaultConfiguration();
        public ReadOnlyObservableCollection<string> AttributeSuggestions { get; }
        public ReadOnlyObservableCollection<string> ValidationMessages { get; }

        public CsvColumnDefinition? SelectedColumn
        {
            get => _selectedColumn;
            set
            {
                if (_selectedColumn == value)
                {
                    return;
                }

                _selectedColumn = value;
                OnPropertyChanged();
                if (RemoveColumnCommand is RelayCommand remove)
                {
                    remove.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand AddColumnCommand { get; }
        public ICommand RemoveColumnCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand BrowseCommand { get; }
#if DEBUG
        public ICommand DebugSaveCommand { get; }
#endif

        public event Action? RequestClose;

        public CsvViewerViewModel(IFileDialogService fileDialog, IMessageRoutingService routingService, string? configPath = null)
        {
            _fileDialog = fileDialog ?? throw new ArgumentNullException(nameof(fileDialog));
            _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
            _configPath = configPath ?? "csv_config.json";
            AttributeSuggestions = new ReadOnlyObservableCollection<string>(_attributeSuggestions);
            ValidationMessages = new ReadOnlyObservableCollection<string>(_validationMessages);
            _routingService.AttributeChanged += OnRoutingAttributeChanged;
            Load();
            AddColumnCommand = new RelayCommand(AddColumn);
            RemoveColumnCommand = new RelayCommand(RemoveSelectedColumn, () => SelectedColumn != null);
            SaveCommand = new RelayCommand(Save);
            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());
            BrowseCommand = new RelayCommand(BrowseDirectory);
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
                    Configuration = JsonSerializer.Deserialize<CsvConfiguration>(json) ?? CreateDefaultConfiguration();
                    EnsureObservableColumns();
                    OnPropertyChanged(nameof(Configuration));
                    RefreshValidationMessages();
                    return;
                }
            }

            EnsureObservableColumns();
            RefreshValidationMessages();
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
                    OutputDirectory = Configuration.OutputDirectory,
                    Columns = Configuration.Columns
                        .Select(c => new CsvColumnDefinition
                        {
                            Name = c.Name,
                            Expression = c.Expression,
                            Format = c.Format
                        })
                        .ToList()
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

        private void BrowseDirectory()
        {
            var path = _fileDialog.SelectFolder();
            if (!string.IsNullOrWhiteSpace(path))
            {
                Configuration.OutputDirectory = path;
                OnPropertyChanged(nameof(Configuration));
            }
        }

        // Uses OnPropertyChanged from ViewModelBase

        private static CsvConfiguration CreateDefaultConfiguration()
        {
            return new CsvConfiguration
            {
                Columns = new ObservableCollection<CsvColumnDefinition>()
            };
        }

        private void EnsureObservableColumns()
        {
            ObservableCollection<CsvColumnDefinition> columns;
            if (Configuration.Columns is ObservableCollection<CsvColumnDefinition> existing)
            {
                columns = existing;
            }
            else
            {
                var seed = Configuration.Columns ?? new List<CsvColumnDefinition>();
                columns = new ObservableCollection<CsvColumnDefinition>(seed);
                Configuration.Columns = columns;
            }

            AttachColumns(columns);
        }

        private void AttachColumns(ObservableCollection<CsvColumnDefinition> columns)
        {
            if (_observableColumns is not null)
            {
                _observableColumns.CollectionChanged -= OnColumnsChanged;
                foreach (var column in _observableColumns)
                {
                    column.PropertyChanged -= OnColumnPropertyChanged;
                }
            }

            _observableColumns = columns;
            _observableColumns.CollectionChanged += OnColumnsChanged;

            foreach (var column in _observableColumns)
            {
                column.PropertyChanged += OnColumnPropertyChanged;
                RegisterColumn(column);
            }
        }

        private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            if (e.OldItems is not null)
            {
                foreach (CsvColumnDefinition item in e.OldItems)
                {
                    item.PropertyChanged -= OnColumnPropertyChanged;
                }
            }

            if (e.NewItems is not null)
            {
                foreach (CsvColumnDefinition item in e.NewItems)
                {
                    item.PropertyChanged += OnColumnPropertyChanged;
                    RegisterColumn(item);
                }
            }

            RefreshValidationMessages();
        }

        private void OnColumnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is CsvColumnDefinition column)
            {
                RegisterColumn(column);
            }

            RefreshValidationMessages();
        }

        private void RegisterColumn(CsvColumnDefinition column)
        {
            foreach (var reference in CsvExpressionEvaluator.GetReferences(column.Expression))
            {
                AddSuggestion(reference.Service, reference.Attribute);
            }
        }

        private void AddColumn()
        {
            var column = new CsvColumnDefinition();
            Configuration.Columns.Add(column);
            SelectedColumn = column;
        }

        private void RemoveSelectedColumn()
        {
            if (SelectedColumn is null)
            {
                return;
            }

            Configuration.Columns.Remove(SelectedColumn);
            SelectedColumn = null;
        }

        private void RefreshValidationMessages()
        {
            _validationMessages.Clear();
            if (_observableColumns is null)
            {
                return;
            }

            foreach (var column in _observableColumns)
            {
                if (string.IsNullOrWhiteSpace(column.Expression))
                {
                    _validationMessages.Add($"Column '{column.Name}' requires an attribute expression.");
                    continue;
                }

                var references = CsvExpressionEvaluator.GetReferences(column.Expression);
                if (references.Count == 0)
                {
                    _validationMessages.Add($"Column '{column.Name}' does not reference any attributes. Use tokens such as {{Service.InputMessage}}.");
                    continue;
                }

                foreach (var reference in references)
                {
                    if (!_routingService.TryGetAttribute(reference.Service, reference.Attribute, out _))
                    {
                        _validationMessages.Add($"Column '{column.Name}' references {reference.Service}.{reference.Attribute}, but no value is published yet.");
                    }
                }
            }

            OnPropertyChanged(nameof(ValidationMessages));
        }

        private void AddSuggestion(string serviceName, string attributeName)
        {
            if (string.IsNullOrWhiteSpace(serviceName) || string.IsNullOrWhiteSpace(attributeName))
            {
                return;
            }

            var token = $"{{{serviceName}.{attributeName}}}";
            if (_suggestionSet.Add(token))
            {
                _attributeSuggestions.Add(token);
                OnPropertyChanged(nameof(AttributeSuggestions));
            }
        }

        private void OnRoutingAttributeChanged(object? sender, ServiceAttributeChangedEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            RunOnUiThread(() =>
            {
                AddSuggestion(e.ServiceName, e.AttributeName);
                RefreshValidationMessages();
            });
        }

        private static void RunOnUiThread(Action action)
        {
            if (action is null)
            {
                return;
            }

            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.Invoke(action);
            }
        }
    }
}
