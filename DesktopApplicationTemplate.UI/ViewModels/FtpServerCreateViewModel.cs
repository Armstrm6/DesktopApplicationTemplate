using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating an FTP server.
/// </summary>
public class FtpServerCreateViewModel : ValidatableViewModelBase, ILoggingViewModel
{
    private string _serviceName = string.Empty;
    private int _port = 21;
    private string _rootPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpServerCreateViewModel"/> class.
    /// </summary>
    public FtpServerCreateViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        SaveCommand = new RelayCommand(Save);
        AdvancedConfigCommand = new RelayCommand(OpenAdvancedConfig);
    }

    /// <summary>
    /// Current advanced options.
    /// </summary>
    public FtpServerOptions Options { get; } = new();

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }
    /// <summary>
    /// Command for saving the server configuration.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command for launching the advanced configuration view.
    /// </summary>
    public ICommand AdvancedConfigCommand { get; }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, FtpServerOptions>? ServerCreated;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<FtpServerOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Display name for the server.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set
        {
            _serviceName = value;
            if (string.IsNullOrWhiteSpace(value))
                AddError(nameof(ServiceName), "Service name is required");
            else
                ClearErrors(nameof(ServiceName));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Port to listen on.
    /// </summary>
    public int Port
    {
        get => _port;
        set
        {
            _port = value;
            if (value < 1 || value > 65535)
                AddError(nameof(Port), "Port must be between 1 and 65535");
            else
                ClearErrors(nameof(Port));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Root directory for the server.
    /// </summary>
    public string RootPath
    {
        get => _rootPath;
        set
        {
            _rootPath = value;
            if (string.IsNullOrWhiteSpace(value))
                AddError(nameof(RootPath), "Root path is required");
            else
                ClearErrors(nameof(RootPath));
            OnPropertyChanged();
        }
    }

    private void Save()
    {
        if (HasErrors)
            return;
        Logger?.Log("FTP server create options start", LogLevel.Debug);
        Options.Port = Port;
        Options.RootPath = RootPath;
        Logger?.Log("FTP server create options finished", LogLevel.Debug);
        ServerCreated?.Invoke(ServiceName, Options);
    }

    private void OpenAdvancedConfig()
    {
        Logger?.Log("Opening FTP server advanced config", LogLevel.Debug);
        AdvancedConfigRequested?.Invoke(Options);
    }
}
