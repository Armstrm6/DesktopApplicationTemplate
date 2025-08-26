using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating a new CSV creator service.
/// </summary>
public class CsvCreateServiceViewModel : ViewModelBase, ILoggingViewModel
{
    private string _serviceName = string.Empty;
    private string _outputPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvCreateServiceViewModel"/> class.
    /// </summary>
    public CsvCreateServiceViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        CreateCommand = new RelayCommand(Create);
        CancelCommand = new RelayCommand(Cancel);
        AdvancedConfigCommand = new RelayCommand(OpenAdvancedConfig);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Raised when the service is created.
    /// </summary>
    public event Action<string, CsvServiceOptions>? ServiceCreated;

    /// <summary>
    /// Raised when creation is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<CsvServiceOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Command to create the service.
    /// </summary>
    public ICommand CreateCommand { get; }

    /// <summary>
    /// Command to cancel creation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command to open advanced configuration.
    /// </summary>
    public ICommand AdvancedConfigCommand { get; }

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Output directory for CSV files.
    /// </summary>
    public string OutputPath
    {
        get => _outputPath;
        set { _outputPath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public CsvServiceOptions Options { get; } = new();

    private void Create()
    {
        Logger?.Log("CSV create options start", LogLevel.Debug);
        Options.OutputPath = OutputPath;
        Logger?.Log("CSV create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, Options);
    }

    private void Cancel()
    {
        Logger?.Log("CSV create cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    private void OpenAdvancedConfig()
    {
        Logger?.Log("CSV advanced config requested", LogLevel.Debug);
        Options.OutputPath = OutputPath;
        AdvancedConfigRequested?.Invoke(Options);
    }
}
