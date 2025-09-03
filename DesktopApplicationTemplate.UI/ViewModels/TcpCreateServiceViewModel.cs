using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for configuring a new TCP service before creation.
/// </summary>
public class TcpCreateServiceViewModel : ServiceCreateViewModelBase<TcpServiceOptions>
{
    private string _host = string.Empty;
    private int _port;

    /// <summary>
    /// Current advanced options.
    /// </summary>
    public TcpServiceOptions Options { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpCreateServiceViewModel"/> class.
    /// </summary>
    public TcpCreateServiceViewModel(IServiceRule rule, ILoggingService? logger = null)
        : base(rule, logger)
    {
    }

    /// <summary>
    /// Host name or address for the service.
    /// </summary>
    public string Host
    {
        get => _host;
        set
        {
            _host = value;
            var error = Rule.ValidateRequired(value, "Host");
            if (error is not null)
                AddError(nameof(Host), error);
            else
                ClearErrors(nameof(Host));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Port number for the service.
    /// </summary>
    public int Port
    {
        get => _port;
        set
        {
            _port = value;
            var error = Rule.ValidatePort(value);
            if (error is not null)
                AddError(nameof(Port), error);
            else
                ClearErrors(nameof(Port));
            OnPropertyChanged();
        }
    }

    /// <inheritdoc />
    protected override void OnSave()
    {
        if (HasErrors)
        {
            Logger?.Log("TCP create validation failed", LogLevel.Warning);
            return;
        }
        Logger?.Log("TCP create options start", LogLevel.Debug);
        Options.Host = Host;
        Options.Port = Port;
        Logger?.Log("TCP create options finished", LogLevel.Debug);
        RaiseServiceSaved(Options);
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        Logger?.Log("TCP create options cancelled", LogLevel.Debug);
        RaiseEditCancelled();
    }

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        Logger?.Log("Opening TCP advanced config", LogLevel.Debug);
        Options.Host = Host;
        Options.Port = Port;
        RaiseAdvancedConfigRequested(Options);
    }
}
