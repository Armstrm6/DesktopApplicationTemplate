using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing FTP server configuration.
/// </summary>
public class FtpServerEditViewModel : ServiceEditViewModelBase<FtpServerOptions>
{
    private readonly FtpServerOptions _options;
    private string _serviceName;
    private int _port;
    private string _rootPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpServerEditViewModel"/> class.
    /// </summary>
    public FtpServerEditViewModel(string serviceName, FtpServerOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _port = options.Port;
        _rootPath = options.RootPath;
    }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, FtpServerOptions>? ServerUpdated;

    /// <summary>
    /// Raised when editing is cancelled.
    /// </summary>
    public event Action? Cancelled;

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

    /// <inheritdoc />
    protected override void OnSave()
    {
        if (HasErrors)
            return;
        Logger?.Log("FTP server edit options start", LogLevel.Debug);
        _options.Port = Port;
        _options.RootPath = RootPath;
        Logger?.Log("FTP server edit options finished", LogLevel.Debug);
        ServerUpdated?.Invoke(ServiceName, _options);
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        Logger?.Log("FTP server edit options cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        Logger?.Log("Opening FTP server advanced config", LogLevel.Debug);
        AdvancedConfigRequested?.Invoke(_options);
    }
}

