using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced CSV creator configuration.
/// </summary>
public class CsvAdvancedConfigViewModel : ViewModelBase, ILoggingViewModel
{
    private readonly CsvServiceOptions _options;
    private string _delimiter;
    private bool _includeHeaders;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvAdvancedConfigViewModel"/> class.
    /// </summary>
    public CsvAdvancedConfigViewModel(CsvServiceOptions options, ILoggingService? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _delimiter = options.Delimiter;
        _includeHeaders = options.IncludeHeaders;
        Logger = logger;
        SaveCommand = new RelayCommand(Save);
        BackCommand = new RelayCommand(Back);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Command to save the configuration.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command to navigate back without saving.
    /// </summary>
    public ICommand BackCommand { get; }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<CsvServiceOptions>? Saved;

    /// <summary>
    /// Raised when navigation back is requested.
    /// </summary>
    public event Action? BackRequested;

    /// <summary>
    /// Delimiter used between values.
    /// </summary>
    public string Delimiter
    {
        get => _delimiter;
        set { _delimiter = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether to include a header row in generated files.
    /// </summary>
    public bool IncludeHeaders
    {
        get => _includeHeaders;
        set { _includeHeaders = value; OnPropertyChanged(); }
    }

    private void Save()
    {
        Logger?.Log("CSV advanced options start", LogLevel.Debug);
        _options.Delimiter = Delimiter;
        _options.IncludeHeaders = IncludeHeaders;
        Logger?.Log("CSV advanced options finished", LogLevel.Debug);
        Saved?.Invoke(_options);
    }

    private void Back()
    {
        Logger?.Log("CSV advanced options back", LogLevel.Debug);
        BackRequested?.Invoke();
    }
}
