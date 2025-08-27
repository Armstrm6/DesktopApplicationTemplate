using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing File Observer service configuration.
/// </summary>
public class FileObserverEditServiceViewModel : ServiceEditViewModelBase<FileObserverServiceOptions>
{
    private readonly FileObserverServiceOptions _options;
    private string _serviceName;
    private string _filePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverEditServiceViewModel"/> class.
    /// </summary>
    public FileObserverEditServiceViewModel(string serviceName, FileObserverServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _filePath = options.FilePath;
    }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, FileObserverServiceOptions>? ServiceUpdated;

    /// <summary>
    /// Raised when editing is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<FileObserverServiceOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// File path to observe.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set { _filePath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Options being edited.
    /// </summary>
    public FileObserverServiceOptions Options => _options;

    /// <inheritdoc />
    protected override void OnSave()
    {
        _options.FilePath = FilePath;
        ServiceUpdated?.Invoke(ServiceName, _options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => Cancelled?.Invoke();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => AdvancedConfigRequested?.Invoke(_options);
}

