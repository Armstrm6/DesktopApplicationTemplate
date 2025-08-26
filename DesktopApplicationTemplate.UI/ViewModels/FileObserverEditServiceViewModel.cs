using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing File Observer service configuration.
/// </summary>
public class FileObserverEditServiceViewModel : FileObserverCreateServiceViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverEditServiceViewModel"/> class.
    /// </summary>
    public FileObserverEditServiceViewModel(string serviceName, FileObserverServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        ServiceName = serviceName;
        FilePath = options.FilePath;
        Options.FilePath = options.FilePath;
        Options.ImageNames = options.ImageNames;
        Options.SendAllImages = options.SendAllImages;
        Options.SendFirstX = options.SendFirstX;
        Options.XCount = options.XCount;
        Options.SendTcpCommand = options.SendTcpCommand;
        Options.TcpCommand = options.TcpCommand;
    }

    /// <summary>
    /// Command for saving the updated configuration.
    /// </summary>
    public ICommand SaveCommand => CreateCommand;

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, FileObserverServiceOptions>? ServiceUpdated
    {
        add => ServiceCreated += value;
        remove => ServiceCreated -= value;
    }
}
