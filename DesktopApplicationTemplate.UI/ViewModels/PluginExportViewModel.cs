using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model that orchestrates plug-in export operations.
/// </summary>
public sealed class PluginExportViewModel : ViewModelBase, IDisposable
{
    private readonly IPluginExportService _exportService;
    private readonly IServiceCatalog _serviceCatalog;
    private readonly AsyncRelayCommand _exportCommand;
    private readonly RelayCommand _cancelCommand;

    private string _pluginId = "Custom.Plugin";
    private string _pluginName = "Custom Plugin";
    private string _version = "1.0.0";
    private string _destinationPath = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isExporting;

    public PluginExportViewModel(IPluginExportService exportService, IServiceCatalog serviceCatalog)
    {
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _serviceCatalog = serviceCatalog ?? throw new ArgumentNullException(nameof(serviceCatalog));

        _exportCommand = new AsyncRelayCommand(ExportAsync, CanExport);
        _cancelCommand = new RelayCommand(OnCancel);

        Descriptors = new ObservableCollection<PluginDescriptorSelectionViewModel>();
        RefreshDescriptors();
        _serviceCatalog.DescriptorsChanged += OnDescriptorsChanged;
    }

    public ObservableCollection<PluginDescriptorSelectionViewModel> Descriptors { get; }

    public ICommand ExportCommand => _exportCommand;

    public ICommand CancelCommand => _cancelCommand;

    public string PluginId
    {
        get => _pluginId;
        set
        {
            if (_pluginId != value)
            {
                _pluginId = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SuggestedFileName));
            }
        }
    }

    public string PluginName
    {
        get => _pluginName;
        set
        {
            if (_pluginName != value)
            {
                _pluginName = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
    }

    public string Version
    {
        get => _version;
        set
        {
            if (_version != value)
            {
                _version = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SuggestedFileName));
            }
        }
    }

    public string DestinationPath
    {
        get => _destinationPath;
        set
        {
            if (_destinationPath != value)
            {
                _destinationPath = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsExporting
    {
        get => _isExporting;
        private set
        {
            if (_isExporting != value)
            {
                _isExporting = value;
                OnPropertyChanged();
                UpdateCommandStates();
            }
        }
    }

    public bool HasDescriptors => Descriptors.Count > 0;

    public string SuggestedFileName => FormattableString.Invariant($"{PluginPackageUtilities.SanitizeSegment(string.IsNullOrWhiteSpace(PluginId) ? "Plugin" : PluginId.Trim())}-{PluginPackageUtilities.SanitizeSegment(string.IsNullOrWhiteSpace(Version) ? "1.0.0" : Version.Trim())}.peakiot");

    public event EventHandler<PluginExportResult>? ExportCompleted;

    public event EventHandler? CancelRequested;

    public void Dispose()
    {
        foreach (var descriptor in Descriptors)
        {
            descriptor.SelectionChanged -= DescriptorSelectionChanged;
        }

        _serviceCatalog.DescriptorsChanged -= OnDescriptorsChanged;
    }

    private bool CanExport()
    {
        return !IsExporting && Descriptors.Any(descriptor => descriptor.IsSelected);
    }

    private async Task ExportAsync()
    {
        if (IsExporting)
        {
            return;
        }

        IsExporting = true;
        StatusMessage = "Exporting plug-in...";

        try
        {
            var selected = Descriptors
                .Where(descriptor => descriptor.IsSelected)
                .Select(descriptor => descriptor.Id)
                .ToArray();

            var request = new PluginExportRequest(
                PluginId,
                PluginName,
                Version,
                selected,
                string.IsNullOrWhiteSpace(DestinationPath) ? null : DestinationPath);

            var result = await _exportService.ExportAsync(request).ConfigureAwait(true);
            StatusMessage = result.Message;

            if (result.Success && !string.IsNullOrWhiteSpace(result.PackagePath))
            {
                DestinationPath = result.PackagePath;
            }

            ExportCompleted?.Invoke(this, result);
        }
        finally
        {
            IsExporting = false;
        }
    }

    private void OnCancel()
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnDescriptorsChanged(object? sender, EventArgs e)
    {
        if (App.UiThreadTaskFactory is null)
        {
            RefreshDescriptors();
            return;
        }

        _ = App.UiThreadTaskFactory.RunAsync(async () =>
        {
            RefreshDescriptors();
            await Task.CompletedTask;
        });
    }

    private void RefreshDescriptors()
    {
        var previouslySelected = new HashSet<string>(Descriptors.Where(d => d.IsSelected).Select(d => d.Id), StringComparer.Ordinal);
        foreach (var descriptor in Descriptors.ToList())
        {
            descriptor.SelectionChanged -= DescriptorSelectionChanged;
        }

        Descriptors.Clear();

        foreach (var info in _exportService.GetExportableDescriptors())
        {
            var descriptor = new PluginDescriptorSelectionViewModel(info.Id, info.DisplayName, info.Category)
            {
                IsSelected = previouslySelected.Contains(info.Id),
            };
            descriptor.SelectionChanged += DescriptorSelectionChanged;
            Descriptors.Add(descriptor);
        }

        OnPropertyChanged(nameof(HasDescriptors));
        UpdateCommandStates();
    }

    private void DescriptorSelectionChanged(object? sender, EventArgs e)
    {
        UpdateCommandStates();
    }

    private void UpdateCommandStates()
    {
        _exportCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Represents a selectable descriptor in the export dialog.
    /// </summary>
    public sealed class PluginDescriptorSelectionViewModel : ViewModelBase
    {
        private bool _isSelected;

        public PluginDescriptorSelectionViewModel(string id, string displayName, string category)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public string Category { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler? SelectionChanged;
    }
}
