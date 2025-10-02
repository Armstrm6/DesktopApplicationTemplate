using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels.FileObserver.Create;

/// <summary>
/// View model for creating a new File Observer service.
/// </summary>
public class FileObserverCreateServiceViewModel : ServiceCreateViewModelBase<FileObserverServiceOptions>
{
    private readonly IFileDialogService _fileDialog;
    private string _filePath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverCreateServiceViewModel"/> class.
    /// </summary>
    public FileObserverCreateServiceViewModel(IServiceRule rule, IFileDialogService fileDialog, ILoggingService? logger = null)
        : base(rule, logger: logger)
    {
        _fileDialog = fileDialog ?? throw new ArgumentNullException(nameof(fileDialog));
        BrowseCommand = new RelayCommand(BrowseFolder);
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
    /// Command to browse for a folder.
    /// </summary>
    public ICommand BrowseCommand { get; }

    /// <inheritdoc />
    protected override void ApplyOptions(FileObserverServiceOptions options)
    {
        options.FilePath = FilePath;
    }

    private void BrowseFolder()
    {
        var path = _fileDialog.SelectFolder();
        if (!string.IsNullOrWhiteSpace(path))
        {
            FilePath = path;
        }
    }
}
