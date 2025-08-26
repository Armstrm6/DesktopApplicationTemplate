using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced HID configuration.
/// </summary>
public class HidAdvancedConfigViewModel : ValidatableViewModelBase, ILoggingViewModel
{
    private readonly HidServiceOptions _options;
    private int _debounceTime;
    private int _keyDownTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="HidAdvancedConfigViewModel"/> class.
    /// </summary>
    public HidAdvancedConfigViewModel(HidServiceOptions options, ILoggingService? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _debounceTime = options.DebounceTimeMs;
        _keyDownTime = options.KeyDownTimeMs;
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
    public event Action<HidServiceOptions>? Saved;

    /// <summary>
    /// Raised when navigation back is requested.
    /// </summary>
    public event Action? BackRequested;

    /// <summary>
    /// Debounce time in milliseconds.
    /// </summary>
    public int DebounceTimeMs
    {
        get => _debounceTime;
        set { _debounceTime = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Key down duration in milliseconds.
    /// </summary>
    public int KeyDownTimeMs
    {
        get => _keyDownTime;
        set { _keyDownTime = value; OnPropertyChanged(); }
    }

    private void Save()
    {
        Logger?.Log("HID advanced options start", LogLevel.Debug);
        _options.DebounceTimeMs = DebounceTimeMs;
        _options.KeyDownTimeMs = KeyDownTimeMs;
        Logger?.Log("HID advanced options finished", LogLevel.Debug);
        Saved?.Invoke(_options);
    }

    private void Back()
    {
        Logger?.Log("HID advanced options back", LogLevel.Debug);
        BackRequested?.Invoke();
    }
}
