using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced HID configuration.
/// </summary>
public class HidAdvancedConfigViewModel : AdvancedConfigViewModelBase<HidServiceOptions>
{
    private readonly HidServiceOptions _options;
    private int _debounceTime;
    private int _keyDownTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="HidAdvancedConfigViewModel"/> class.
    /// </summary>
    public HidAdvancedConfigViewModel(HidServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _debounceTime = options.DebounceTimeMs;
        _keyDownTime = options.KeyDownTimeMs;
    }

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

    protected override HidServiceOptions OnSave()
    {
        Logger?.Log("HID advanced options start", LogLevel.Debug);
        _options.DebounceTimeMs = DebounceTimeMs;
        _options.KeyDownTimeMs = KeyDownTimeMs;
        Logger?.Log("HID advanced options finished", LogLevel.Debug);
        return _options;
    }

    protected override void OnBack()
    {
        Logger?.Log("HID advanced options back", LogLevel.Debug);
    }
}
