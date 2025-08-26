using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced Heartbeat configuration.
/// </summary>
public class HeartbeatAdvancedConfigViewModel : ValidatableViewModelBase, ILoggingViewModel
{
    private readonly HeartbeatServiceOptions _options;
    private bool _includePing;
    private bool _includeStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeartbeatAdvancedConfigViewModel"/> class.
    /// </summary>
    public HeartbeatAdvancedConfigViewModel(HeartbeatServiceOptions options, ILoggingService? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _includePing = options.IncludePing;
        _includeStatus = options.IncludeStatus;
        Logger = logger;
        SaveCommand = new RelayCommand(Save);
        BackCommand = new RelayCommand(Back);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Command to save the configuration.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command to navigate back without saving.
    /// </summary>
    public ICommand BackCommand { get; }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<HeartbeatServiceOptions>? Saved;

    /// <summary>
    /// Raised when navigation back is requested.
    /// </summary>
    public event Action? BackRequested;

    /// <summary>
    /// Whether to include a ping indicator in the heartbeat.
    /// </summary>
    public bool IncludePing
    {
        get => _includePing;
        set { _includePing = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether to include a status indicator in the heartbeat.
    /// </summary>
    public bool IncludeStatus
    {
        get => _includeStatus;
        set { _includeStatus = value; OnPropertyChanged(); }
    }

    private void Save()
    {
        Logger?.Log("Heartbeat advanced options start", LogLevel.Debug);
        _options.IncludePing = IncludePing;
        _options.IncludeStatus = IncludeStatus;
        Logger?.Log("Heartbeat advanced options finished", LogLevel.Debug);
        Saved?.Invoke(_options);
    }

    private void Back()
    {
        Logger?.Log("Heartbeat advanced options back", LogLevel.Debug);
        BackRequested?.Invoke();
    }
}
