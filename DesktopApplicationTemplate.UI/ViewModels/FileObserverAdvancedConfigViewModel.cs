using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for advanced configuration of a file observer service.
/// </summary>
public class FileObserverAdvancedConfigViewModel : ViewModelBase, ILoggingViewModel
{
    private readonly FileObserverOptions _options;
    private bool _includeSubdirectories;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverAdvancedConfigViewModel"/> class.
    /// </summary>
    public FileObserverAdvancedConfigViewModel(FileObserverOptions options, ILoggingService? logger = null)
    {
        _options = options;
        _includeSubdirectories = options.IncludeSubdirectories;
        Logger = logger;
        SaveCommand = new RelayCommand(Save);
        BackCommand = new RelayCommand(Back);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Raised when configuration is saved.
    /// </summary>
    public event Action<FileObserverOptions>? Saved;

    /// <summary>
    /// Raised when back navigation is requested.
    /// </summary>
    public event Action? BackRequested;

    /// <summary>
    /// Whether to include subdirectories.
    /// </summary>
    public bool IncludeSubdirectories
    {
        get => _includeSubdirectories;
        set { _includeSubdirectories = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Command to save options.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command to navigate back.
    /// </summary>
    public ICommand BackCommand { get; }

    private void Save()
    {
        Logger?.Log("Saving file observer advanced options", LogLevel.Debug);
        _options.IncludeSubdirectories = IncludeSubdirectories;
        Saved?.Invoke(_options);
    }

    private void Back()
    {
        Logger?.Log("Back from file observer advanced options", LogLevel.Debug);
        BackRequested?.Invoke();
    }
}

