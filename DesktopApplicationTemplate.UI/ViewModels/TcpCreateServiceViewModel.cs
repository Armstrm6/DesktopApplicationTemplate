using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for configuring a new TCP service before creation.
/// </summary>
public class TcpCreateServiceViewModel : ViewModelBase
{
    private string _serviceName = string.Empty;
    private string _host = string.Empty;
    private int _port = 0;
    private bool _useUdp;
    private TcpServiceMode _mode = TcpServiceMode.Listening;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpCreateServiceViewModel"/> class.
    /// </summary>
    public TcpCreateServiceViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        CreateCommand = new RelayCommand(Create);
        CancelCommand = new RelayCommand(Cancel);
        Modes = Enum.GetValues(typeof(TcpServiceMode)).Cast<TcpServiceMode>().ToArray();
    }

    /// <summary>
    /// Raised when the user finishes configuring the service.
    /// </summary>
    public event Action<string, TcpServiceOptions>? ServiceCreated;

    /// <summary>
    /// Raised when the user cancels configuration.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Command to finalize service creation.
    /// </summary>
    public ICommand CreateCommand { get; }

    /// <summary>
    /// Command to cancel configuration.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Available service modes.
    /// </summary>
    public IReadOnlyList<TcpServiceMode> Modes { get; }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Name of the service to create.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Host name or address for the service.
    /// </summary>
    public string Host
    {
        get => _host;
        set { _host = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Port number for the service.
    /// </summary>
    public int Port
    {
        get => _port;
        set { _port = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether the service should use UDP instead of TCP.
    /// </summary>
    public bool UseUdp
    {
        get => _useUdp;
        set { _useUdp = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Operating mode for the service.
    /// </summary>
    public TcpServiceMode Mode
    {
        get => _mode;
        set { _mode = value; OnPropertyChanged(); }
    }

    private void Create()
    {
        Logger?.Log("TCP create options start", LogLevel.Debug);
        var options = new TcpServiceOptions
        {
            Host = Host,
            Port = Port,
            UseUdp = UseUdp,
            Mode = Mode
        };
        Logger?.Log("TCP create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, options);
    }

    private void Cancel()
    {
        Logger?.Log("TCP create options cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }
}
