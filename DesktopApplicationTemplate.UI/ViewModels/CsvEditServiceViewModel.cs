using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing CSV creator service configuration.
/// </summary>
public class CsvEditServiceViewModel : CsvCreateServiceViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvEditServiceViewModel"/> class.
    /// </summary>
    public CsvEditServiceViewModel(string serviceName, CsvServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        ServiceName = serviceName;
        OutputPath = options.OutputPath;
        Options.OutputPath = options.OutputPath;
        Options.Delimiter = options.Delimiter;
        Options.IncludeHeaders = options.IncludeHeaders;
    }

    /// <summary>
    /// Command for saving the updated configuration.
    /// </summary>
    public ICommand SaveCommand => CreateCommand;

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, CsvServiceOptions>? ServiceUpdated
    {
        add => ServiceCreated += value;
        remove => ServiceCreated -= value;
    }
}
