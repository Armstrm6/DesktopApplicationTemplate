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
    private readonly IServiceRule _rule;
    private readonly IServiceScreen<FtpServerOptions> _screen;
    private string _serviceName = string.Empty;
    private int _port = 21;
    private string _rootPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpServerCreateViewModel"/> class.
    /// </summary>
    public FtpServerCreateViewModel(IServiceRule rule, IServiceScreen<FtpServerOptions> screen, ILoggingService? logger = null)
    {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        Logger = logger;

        SaveCommand = new RelayCommand(() =>
        {
            if (HasErrors)
                return;
            Options.Port = Port;
            Options.RootPath = RootPath;
            _screen.Save(ServiceName, Options);
        });
        CancelCommand = new RelayCommand(() => _screen.Cancel());
        AdvancedConfigCommand = new RelayCommand(() =>
        {
            Options.Port = Port;
            Options.RootPath = RootPath;
            _screen.OpenAdvanced(Options);
        });

        _screen.Saved += (n, o) => ServerCreated?.Invoke(n, o);
        _screen.Cancelled += () => Cancelled?.Invoke();
        _screen.AdvancedConfigRequested += o => AdvancedConfigRequested?.Invoke(o);
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
    /// Command for cancelling server creation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command for launching the advanced configuration view.
    /// </summary>
    public ICommand AdvancedConfigCommand { get; }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, FtpServerOptions>? ServerCreated;

    /// <summary>
    /// Raised when creation is cancelled.
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
            var error = _rule.ValidateRequired(value, "Service name");
            if (error is not null)
                AddError(nameof(ServiceName), error);
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
            var error = _rule.ValidatePort(value);
            if (error is not null)
                AddError(nameof(Port), error);
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
            var error = _rule.ValidateRequired(value, "Root path");
            if (error is not null)
                AddError(nameof(RootPath), error);
            else
                ClearErrors(nameof(RootPath));
            OnPropertyChanged();
        }
    }
}
