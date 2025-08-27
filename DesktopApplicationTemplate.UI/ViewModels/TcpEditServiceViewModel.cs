using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing TCP service configuration.
/// </summary>
public class TcpEditServiceViewModel : TcpCreateServiceViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TcpEditServiceViewModel"/> class.
    /// </summary>
    public TcpEditServiceViewModel(ILoggingService? logger = null)
        : base(logger)
    {
    }

    /// <summary>
    /// Loads existing options into the view model for editing.
    /// </summary>
    /// <param name="serviceName">Existing service name.</param>
    /// <param name="options">Options to edit.</param>
    public void Load(string serviceName, TcpServiceOptions options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));

        ServiceName = serviceName;
        Host = options.Host;
        Port = options.Port;
        Options.Host = options.Host;
        Options.Port = options.Port;
        Options.UseUdp = options.UseUdp;
        Options.Mode = options.Mode;
    }

    /// <summary>
    /// Command for saving the updated configuration.
    /// </summary>
    public ICommand SaveCommand => CreateCommand;

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, TcpServiceOptions>? ServiceUpdated
    {
        add => ServiceCreated += value;
        remove => ServiceCreated -= value;
    }
}
