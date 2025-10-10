using System;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;

namespace DesktopApplicationTemplate.UI.ViewModels.FileObserver.Edit;

/// <summary>
/// View model for editing an existing File Observer service configuration.
/// </summary>
public class FileObserverEditServiceViewModel : ServiceEditViewModelBase<FileObserverServiceOptions>
{
    private readonly FileObserverServiceOptions _options;
    private string _filePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverEditServiceViewModel"/> class.
    /// </summary>
    public FileObserverEditServiceViewModel(IServiceRule rule, string serviceName, FileObserverServiceOptions options, ILoggingService? logger = null)
        : base(rule, logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _filePath = options.FilePath;
    }


    /// <summary>
    /// File path to observe.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            var error = Rule.ValidateRequired(value, "File path");
            if (error is not null)
                AddError(nameof(FilePath), error);
            else
                ClearErrors(nameof(FilePath));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Options being edited.
    /// </summary>
    public FileObserverServiceOptions Options => _options;

    /// <inheritdoc />
    protected override Task OnSaveAsync()
    {
        if (HasErrors)
        {
            Logger?.Log("FileObserver edit validation failed", LogLevel.Warning);
            return Task.CompletedTask;
        }
        _options.FilePath = FilePath;
        return RaiseServiceSavedAsync(_options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => RaiseEditCancelled();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => RaiseAdvancedConfigRequested(_options);
}

