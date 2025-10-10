using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.VisualStudio.Threading;

namespace DesktopApplicationTemplate.UI.ViewModels.Csv
{
    public class CsvServiceViewModel : ViewModelBase
    {
        private readonly string _configPath;
        private readonly IFileDialogService _fileDialog;
        private readonly IMessageRoutingService _routingService;
        private readonly ObservableCollection<string> _attributeSuggestions = new();
        private readonly ObservableCollection<string> _validationMessages = new();
        private readonly HashSet<string> _suggestionSet = new(StringComparer.OrdinalIgnoreCase);
        private readonly RelayCommand _addColumnCommand;
        private readonly RelayCommand _removeColumnCommand;
        private readonly RelayCommand _saveCommand;
        private readonly RelayCommand _browseCommand;
#if DEBUG
        private readonly RelayCommand _debugSaveCommand;
#endif
        private readonly JoinableTaskFactory? _joinableTaskFactory;
        private ObservableCollection<CsvColumnDefinition>? _observableColumns;
        private CsvColumnDefinition? _selectedColumn;
        private bool _isBusy;

        public CsvConfiguration Configuration { get; private set; } = CreateDefaultConfiguration();
        public ReadOnlyObservableCollection<string> AttributeSuggestions { get; }
        public ReadOnlyObservableCollection<string> ValidationMessages { get; }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value)
                {
                    return;
                }

                _isBusy = value;
                OnPropertyChanged();
                UpdateCommandStates();
            }
        }

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
                _removeColumnCommand.RaiseCanExecuteChanged();
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

        public CsvServiceViewModel(
            IFileDialogService fileDialog,
            IMessageRoutingService routingService,
            string? configPath = null,
            JoinableTaskFactory? joinableTaskFactory = null)
        {
            _fileDialog = fileDialog ?? throw new ArgumentNullException(nameof(fileDialog));
            _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
            _configPath = configPath ?? "csv_config.json";
            _joinableTaskFactory = joinableTaskFactory ?? App.UiThreadTaskFactory;
            AttributeSuggestions = new ReadOnlyObservableCollection<string>(_attributeSuggestions);
            ValidationMessages = new ReadOnlyObservableCollection<string>(_validationMessages);
            _routingService.AttributeChanged += OnRoutingAttributeChanged;
            _addColumnCommand = new RelayCommand(AddColumn, () => !IsBusy);
            _removeColumnCommand = new RelayCommand(RemoveSelectedColumn, () => SelectedColumn != null && !IsBusy);
            _saveCommand = new RelayCommand(ExecuteSaveAsync, () => !IsBusy);
            _browseCommand = new RelayCommand(BrowseDirectory, () => !IsBusy);
#if DEBUG
            _debugSaveCommand = new RelayCommand(ExecuteSaveAsync, () => !IsBusy);
#endif
            AddColumnCommand = _addColumnCommand;
            RemoveColumnCommand = _removeColumnCommand;
            SaveCommand = _saveCommand;
            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());
            BrowseCommand = _browseCommand;
#if DEBUG
            DebugSaveCommand = _debugSaveCommand;
#endif
            InitializeAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeAsync()
        {
            SetIsBusy(true);
            try
            {
                var configuration = await LoadConfigurationAsync(CancellationToken.None).ConfigureAwait(false);
                RunOnUiThread(() => ApplyConfiguration(configuration));
            }
            catch (Exception ex)
            {
                RunOnUiThread(() => HandleInitializationError(ex));
            }
            finally
            {
                SetIsBusy(false);
            }
        }

        public void Save()
        {
            ExecuteSaveAsync();
        }

        private async void ExecuteSaveAsync()
        {
            if (Configuration is null || IsBusy)
            {
                return;
            }

            SetIsBusy(true);
            try
            {
                await SaveConfigurationAsync(Configuration, CancellationToken.None).ConfigureAwait(false);
            }
            catch (StackOverflowException)
            {
                HandleStackOverflowDuringSave();
            }
            catch (Exception ex)
            {
                RunOnUiThread(() =>
                {
                    _validationMessages.Add($"Failed to save CSV configuration: {ex.Message}");
                    OnPropertyChanged(nameof(ValidationMessages));
                });
            }
            finally
            {
                SetIsBusy(false);
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

        private void ApplyConfiguration(CsvConfiguration configuration)
        {
            Configuration = configuration ?? CreateDefaultConfiguration();
            _suggestionSet.Clear();
            _attributeSuggestions.Clear();
            OnPropertyChanged(nameof(AttributeSuggestions));
            OnPropertyChanged(nameof(Configuration));
            EnsureObservableColumns();
            RefreshValidationMessages();
        }

        private void HandleInitializationError(Exception exception)
        {
            Configuration = CreateDefaultConfiguration();
            EnsureObservableColumns();
            _suggestionSet.Clear();
            _attributeSuggestions.Clear();
            _validationMessages.Clear();
            _validationMessages.Add($"Failed to load CSV configuration: {exception.Message}");
            OnPropertyChanged(nameof(AttributeSuggestions));
            OnPropertyChanged(nameof(Configuration));
            OnPropertyChanged(nameof(ValidationMessages));
        }

        private async Task<CsvConfiguration> LoadConfigurationAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_configPath))
            {
                return CreateDefaultConfiguration();
            }

            var json = await File.ReadAllTextAsync(_configPath, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
            {
                return CreateDefaultConfiguration();
            }

            return await Task.Run(() => JsonSerializer.Deserialize<CsvConfiguration>(json), cancellationToken)
                .ConfigureAwait(false)
                ?? CreateDefaultConfiguration();
        }

        private async Task SaveConfigurationAsync(CsvConfiguration configuration, CancellationToken cancellationToken)
        {
            var snapshot = RunOnUiThread(() => CreateSnapshot(configuration));
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = await Task.Run(() => JsonSerializer.Serialize(snapshot, options), cancellationToken)
                .ConfigureAwait(false);
            await File.WriteAllTextAsync(_configPath, json, cancellationToken).ConfigureAwait(false);
        }

        private CsvConfiguration CreateSnapshot(CsvConfiguration source)
        {
            return new CsvConfiguration
            {
                FileNamePattern = source.FileNamePattern,
                OutputDirectory = source.OutputDirectory,
                Columns = source.Columns
                    .Select(c => new CsvColumnDefinition
                    {
                        Name = c.Name,
                        Expression = c.Expression,
                        Format = c.Format
                    })
                    .ToList()
            };
        }

        private void HandleStackOverflowDuringSave()
        {
            var dumpOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };
            var dump = JsonSerializer.Serialize(Configuration, dumpOptions);
            var temp = Path.Combine(Path.GetTempPath(), "csv_config_dump.json");
            File.WriteAllText(temp, dump);
            Environment.FailFast($"Stack overflow while saving CSV configuration. Dump written to {temp}");
        }

        private void SetIsBusy(bool value)
        {
            RunOnUiThread(() => IsBusy = value);
        }

        private void UpdateCommandStates()
        {
            _addColumnCommand.RaiseCanExecuteChanged();
            _removeColumnCommand.RaiseCanExecuteChanged();
            _saveCommand.RaiseCanExecuteChanged();
            _browseCommand.RaiseCanExecuteChanged();
#if DEBUG
            _debugSaveCommand.RaiseCanExecuteChanged();
#endif
        }

        private void RunOnUiThread(Action action)
        {
            if (action is null)
            {
                return;
            }

            if (_joinableTaskFactory is { } factory)
            {
                if (factory.Context.IsOnMainThread)
                {
                    action();
                    return;
                }

                factory.Run(async () =>
                {
                    await factory.SwitchToMainThreadAsync();
                    action();
                });
                return;
            }

            action();
        }

        private T RunOnUiThread<T>(Func<T> function)
        {
            if (function is null)
            {
                return default!;
            }

            if (_joinableTaskFactory is { } factory)
            {
                if (factory.Context.IsOnMainThread)
                {
                    return function();
                }

                return factory.Run(async () =>
                {
                    await factory.SwitchToMainThreadAsync();
                    return function();
                });
            }

            return function();
        }
    }
}
