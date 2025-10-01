using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using WpfBrushes = System.Windows.Media.Brushes;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Persistence;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels.Tcp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Navigation;
using DesktopApplicationTemplate.UI.Factories;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceListModel> Services { get; set; } = new();
        public ICollectionView FilteredServices { get; }
        public FilterViewModel Filters { get; } = new();
        public ObservableCollection<LogEntry> AllLogs { get; } = new();
        private ServiceListModel? _selectedService;
        public ServiceListModel? SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged();
                LogViewModel.SetLogs(_selectedService?.Logs ?? AllLogs);
                (RemoveServiceCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (EditServiceCommand as RelayCommand<ServiceListModel?>)?.RaiseCanExecuteChanged();
            }
        }

        public enum ImportFeedbackStatus
        {
            Success,
            Error
        }

        public sealed class ImportFeedbackEventArgs : EventArgs
        {
            public ImportFeedbackEventArgs(ImportFeedbackStatus status, string message)
            {
                Status = status;
                Message = message ?? throw new ArgumentNullException(nameof(message));
            }

            public ImportFeedbackStatus Status { get; }

            public string Message { get; }
        }

        public ICommand AddServiceCommand { get; }
        public ICommand RemoveServiceCommand { get; }
        public ICommand EditServiceCommand { get; }
        public ICommand ImportServiceCommand { get; }
        public int ServicesCreated => Services.Count;
        public int CurrentActiveServices => Services.Count(s => s.IsActive);

        public ServiceLogViewModel LogViewModel { get; }

        public LogLevel LogLevelFilter
        {
            get => LogViewModel.LogLevelFilter;
            set => LogViewModel.LogLevelFilter = value;
        }

        public IEnumerable<LogEntry> DisplayLogs => LogViewModel.DisplayLogs;

        private readonly CsvService _csvService;
        private readonly ILoggingService? _logger;
        private readonly INetworkConfigurationService _networkService;
        private readonly IServiceUiRegistry _uiRegistry;
        private readonly IServiceCatalog _catalog;
        private readonly IFileDialogService _fileDialogService;
        private readonly IPluginImportService _pluginImportService;

        public NetworkConfigurationViewModel NetworkConfig { get; }

        public MainViewModel(
            CsvService csvService,
            NetworkConfigurationViewModel networkConfig,
            INetworkConfigurationService networkService,
            IServiceUiRegistry uiRegistry,
            IServiceCatalog catalog,
            IFileDialogService fileDialogService,
            IPluginImportService pluginImportService,
            ILoggingService? logger = null,
            string? servicesFilePath = null)
        {
            _csvService = csvService;
            _networkService = networkService;
            _logger = logger;
            NetworkConfig = networkConfig;
            _uiRegistry = uiRegistry;
            _catalog = catalog;
            _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
            _pluginImportService = pluginImportService ?? throw new ArgumentNullException(nameof(pluginImportService));
            _ = NetworkConfig.LoadAsync();
            _networkService.ConfigurationChanged += (_, cfg) => ApplyNetworkConfiguration(cfg);
            ServiceListModel.ResolveService = (descriptorKey, name) =>
            {
                var normalized = NormalizeDescriptorKey(descriptorKey);
                ServiceType? legacyType = null;
                if (ServiceTypeExtensions.TryParse(descriptorKey, out var parsed))
                {
                    legacyType = parsed;
                }

                return Services.FirstOrDefault(s =>
                    (string.Equals(s.DescriptorId, normalized, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(s.DescriptorId, descriptorKey, StringComparison.OrdinalIgnoreCase) ||
                     (legacyType.HasValue && s.Type == legacyType.Value)) &&
                    s.DisplayName.Split(" - ").Last().Equals(name, StringComparison.OrdinalIgnoreCase));
            };
            if (!string.IsNullOrWhiteSpace(servicesFilePath))
            {
                ServicePersistence.FilePath = servicesFilePath!;
                _logger?.Log($"Using service persistence path {ServicePersistence.FilePath}", LogLevel.Debug);
            }
            AddServiceCommand = new RelayCommand(AddService);
            RemoveServiceCommand = new AsyncRelayCommand(RemoveSelectedServiceAsync, () => SelectedService != null);
            EditServiceCommand = new RelayCommand<ServiceListModel?>(EditService, svc => svc != null);
            ImportServiceCommand = new AsyncRelayCommand(ImportServiceAsync);
            FilteredServices = CollectionViewSource.GetDefaultView(Services);
            Filters.PropertyChanged += (_, __) => ApplyFilters();
            LoadServices();
            ApplyFilters();
            LogViewModel = new ServiceLogViewModel(ServiceType.Mqtt, AllLogs);
            LogViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ServiceLogViewModel.DisplayLogs))
                    OnPropertyChanged(nameof(DisplayLogs));
                if (e.PropertyName == nameof(ServiceLogViewModel.LogLevelFilter))
                    OnPropertyChanged(nameof(LogLevelFilter));
            };
            if (_logger is LoggingService concreteLogger)
            {
                concreteLogger.Reload();
            }
        }

        private void ApplyNetworkConfiguration(NetworkConfiguration config)
        {
            foreach (var svc in Services)
            {
                if (svc.ServicePage?.DataContext is INetworkAwareViewModel navm)
                {
                    navm.UpdateNetworkConfiguration(config);
                }
            }
        }

        public event Action? AddServiceRequested;
        public event EventHandler<ImportFeedbackEventArgs>? ImportFeedback;
        private void EditService(ServiceListModel? service)
        {
            var target = service ?? SelectedService;
            if (target == null)
                return;

            if (TryGetDescriptorId(target.Type, out var descriptorId) && _uiRegistry.EditHandlers.TryGetValue(descriptorId, out var handlerFactory))
            {
                handlerFactory().Edit(target);
            }
            else
            {
                _logger?.Log($"No edit handler registered for {target.Type}", LogLevel.Warning);
            }
        }

        private void AddService()
        {
            _logger?.Log("AddService invoked", LogLevel.Debug);
            AddServiceRequested?.Invoke();
            _logger?.Log("AddService completed", LogLevel.Debug);
        }

        private async Task ImportServiceAsync()
        {
            var selectedPath = _fileDialogService.OpenFile(
                "Service Packages (*.peakiot)|*.peakiot|All Files (*.*)|*.*",
                "Import Service Package");

            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                _logger?.Log("Service import cancelled by user.", LogLevel.Debug);
                return;
            }

            _logger?.Log($"Importing service package from {selectedPath}.", LogLevel.Information);

            PluginImportResult result;
            try
            {
                result = await _pluginImportService.ImportAsync(selectedPath).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                _logger?.Log("Service import cancelled.", LogLevel.Information);
                return;
            }
            catch (Exception ex)
            {
                _logger?.Log($"Service import failed: {ex.Message}", LogLevel.Error);
                ImportFeedback?.Invoke(this, new ImportFeedbackEventArgs(ImportFeedbackStatus.Error, "Import failed. Check logs for details."));
                return;
            }

            if (result.Success)
            {
                _logger?.Log($"Imported service package '{Path.GetFileName(selectedPath)}'. {result.Message}", LogLevel.Information);
                ImportFeedback?.Invoke(this, new ImportFeedbackEventArgs(ImportFeedbackStatus.Success, result.Message));
            }
            else
            {
                _logger?.Log($"Service import reported issues for '{selectedPath}': {result.Message}", LogLevel.Warning);
                ImportFeedback?.Invoke(this, new ImportFeedbackEventArgs(ImportFeedbackStatus.Error, result.Message));
            }
        }

        internal string GenerateServiceName(ServiceType serviceType)
        {
            var typeName = GetDisplayPrefix(serviceType);
            int index = 1;
            foreach (var svc in Services.Where(s => s.Type == serviceType))
            {
                var namePart = svc.DisplayName.Split(" - ").Last();
                if (namePart.StartsWith(typeName) &&
                    int.TryParse(namePart.Substring(typeName.Length), out int n) && n >= index)
                {
                    index = n + 1;
                }
            }
            return $"{typeName}{index}";
        }

        private async Task RemoveSelectedServiceAsync()
        {
            if (SelectedService != null)
            {
                _logger?.Log($"Removing service {SelectedService.DisplayName}", LogLevel.Debug);
                var index = Services.IndexOf(SelectedService);
                SelectedService.AddLog("Service removed", WpfBrushes.Red);
                if (SelectedService.Type != ServiceType.Csv)
                    _csvService.RemoveColumnsForService(SelectedService.DisplayName);
                SelectedService.LogAdded -= OnServiceLogAdded;
                SelectedService.ActiveChanged -= OnServiceActiveChanged;
                Services.Remove(SelectedService);
                if (Services.Count > 0)
                {
                    if (index >= Services.Count) index = Services.Count - 1;
                    SelectedService = Services[index];
                }
                else
                {
                    SelectedService = null;
                }
                OnPropertyChanged(nameof(ServicesCreated));
                OnPropertyChanged(nameof(CurrentActiveServices));
                LogViewModel.RefreshLogs();
                await SaveServicesAsync().ConfigureAwait(false);
                _logger?.Log("Service removed", LogLevel.Debug);
            }
        }

        public async Task SaveServicesAsync()
        {
            // Update order prior to saving
            for (int i = 0; i < Services.Count; i++)
            {
                Services[i].Order = i;
            }
            foreach (var svc in Services)
            {
                if (svc.ServicePage?.DataContext is TcpServiceMessagesViewModel tcpVm)
                {
                    await tcpVm.SaveAsync().ConfigureAwait(false);
                }
            }
            ServicePersistence.Save(Services, _catalog, _logger);
        }

        private void LoadServices()
        {
            var existing = ServicePersistence.Load(_catalog, _logger);
            foreach (var info in existing.OrderBy(i => i.Order))
            {
                var descriptor = ResolveDescriptor(info.DescriptorId, info.ServiceType);
                var descriptorId = descriptor?.Id ?? info.DescriptorId;
                var serviceType = descriptor?.LegacyType ?? info.ServiceType;
                ServiceListModel svc;

                if (!string.IsNullOrWhiteSpace(descriptorId) &&
                    _uiRegistry.Factories.TryGetValue(descriptorId, out var factoryFactory))
                {
                    var factory = factoryFactory();
                    var context = new ServiceFactoryContext(descriptorId, info.DisplayName, info.Payload, descriptor);
                    svc = factory.Create(context);
                    svc.DisplayName = info.DisplayName;
                }
                else
                {
                    svc = new ServiceListModel
                    {
                        DescriptorId = descriptorId,
                        Type = serviceType,
                        DescriptorPayload = info.Payload
                    };
                    svc.ApplyDescriptor(descriptor);
                }

                svc.IsActive = info.IsActive;
                svc.Order = info.Order;
                svc.TotalExecutionTimeMs = info.TotalExecutionTimeMs;
                svc.ExecutionCount = info.ExecutionCount;

                foreach (var a in info.AssociatedServices ?? new List<string>())
                {
                    if (!svc.AssociatedServices.Contains(a))
                    {
                        svc.AssociatedServices.Add(a);
                    }
                }

                if (info.Payload is not null && svc.DescriptorPayload is null)
                {
                    svc.DescriptorPayload = info.Payload;
                }

                svc.LogAdded += OnServiceLogAdded;
                svc.ActiveChanged += OnServiceActiveChanged;
                if (svc.Type != ServiceType.Csv)
                {
                    _csvService.EnsureColumnsForService(svc.DisplayName);
                }

                Services.Add(svc);
                _logger?.Log($"Loaded service {svc.DisplayName}", LogLevel.Debug);
            }
            OnPropertyChanged(nameof(ServicesCreated));
            OnPropertyChanged(nameof(CurrentActiveServices));
        }

        private void ApplyFilters()
        {
            FilteredServices.Filter = obj =>
            {
                if (obj is not ServiceListModel svc)
                    return false;

                if (!string.IsNullOrWhiteSpace(Filters.NameFilter) &&
                    !svc.DisplayName.Contains(Filters.NameFilter, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (Filters.TypeFilter != "All" &&
                    ServiceTypeExtensions.TryParse(Filters.TypeFilter, out var fType) &&
                    svc.Type != fType)
                    return false;

                if (Filters.StatusFilter == "Active" && !svc.IsActive)
                    return false;
                if (Filters.StatusFilter == "Inactive" && svc.IsActive)
                    return false;

                return true;
            };
            FilteredServices.Refresh();
        }

        private IServiceDescriptor? ResolveDescriptor(string? descriptorId, ServiceType serviceType)
        {
            if (!string.IsNullOrWhiteSpace(descriptorId) && _catalog.TryGetById(descriptorId!, out var descriptor))
            {
                return descriptor;
            }

            if (_catalog.TryGetByLegacyType(serviceType, out descriptor))
            {
                return descriptor;
            }

            if (_catalog.LegacyMap.TryGetValue(serviceType, out var mappedId) &&
                _catalog.TryGetById(mappedId, out descriptor))
            {
                return descriptor;
            }

            return null;
        }

        private string NormalizeDescriptorKey(string descriptorKey)
        {
            if (string.IsNullOrWhiteSpace(descriptorKey))
            {
                return descriptorKey;
            }

            if (_catalog.TryGetById(descriptorKey, out var descriptor))
            {
                return descriptor.Id;
            }

            if (ServiceTypeExtensions.TryParse(descriptorKey, out var legacy))
            {
                if (_catalog.LegacyMap.TryGetValue(legacy, out var mappedId))
                {
                    return mappedId;
                }

                if (_catalog.TryGetByLegacyType(legacy, out descriptor))
                {
                    return descriptor.Id;
                }

                return legacy.ToDescriptorId();
            }

            return descriptorKey;
        }

        private string GetDisplayPrefix(ServiceType serviceType)
        {
            var descriptor = ResolveDescriptor(null, serviceType);
            if (descriptor is not null)
            {
                var presentation = descriptor.Presentation;
                if (!string.IsNullOrWhiteSpace(presentation.DisplayLabel))
                {
                    return presentation.DisplayLabel!;
                }

                if (!string.IsNullOrWhiteSpace(descriptor.DisplayName))
                {
                    return descriptor.DisplayName;
                }
            }

            return serviceType.ToLegacyString();
        }

        public void OnServiceLogAdded(ServiceListModel svc, LogEntry entry)
        {
            AllLogs.Insert(0, entry);
            if (svc.Type != ServiceType.Csv && Services.Any(s => s.Type == ServiceType.Csv))
            {
                try
                {
                    _csvService.RecordLog(svc.DisplayName, entry.Message);
                }
                catch
                {
                    // ignore CSV errors during logging
                }
            }
            LogViewModel.RefreshLogs();
        }

        internal void OnServiceActiveChanged(bool _)
        {
            OnPropertyChanged(nameof(CurrentActiveServices));
            OnPropertyChanged(nameof(ServicesCreated));
        }

        public void ClearLogs()
        {
            LogViewModel.ClearLogs();
            _logger?.Log("Logs cleared", LogLevel.Debug);
        }

        private bool TryGetDescriptorId(ServiceType serviceType, out string descriptorId)
        {
            if (_catalog.TryGetByLegacyType(serviceType, out var descriptor))
            {
                descriptorId = descriptor.Id;
                return true;
            }

            if (_catalog.LegacyMap.TryGetValue(serviceType, out var mappedId) &&
                !string.IsNullOrWhiteSpace(mappedId))
            {
                descriptorId = mappedId;
                return true;
            }

            descriptorId = serviceType.ToDescriptorId();
            return false;
        }

        public void ExportDisplayedLogs(string filePath)
        {
            LogViewModel.ExportLogs(filePath);
            _logger?.Log($"Exported {LogViewModel.DisplayLogs.Count()} logs to {filePath}", LogLevel.Debug);
        }

        public void RefreshLogs()
        {
            LogViewModel.RefreshLogs();
            _logger?.Log("Logs refreshed", LogLevel.Debug);
        }

        // OnPropertyChanged inherited from ViewModelBase
    }

}
