using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced TCP configuration.
/// </summary>
public class TcpAdvancedConfigViewModel : ViewModelBase, ILoggingViewModel
{
    private TcpServiceOptions? _options;
    private bool _useUdp;
    private TcpServiceMode _mode;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpAdvancedConfigViewModel"/> class.
    /// </summary>
    public TcpAdvancedConfigViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        SaveCommand = new RelayCommand(Save);
        BackCommand = new RelayCommand(Back);
    }

    /// <summary>
    /// Loads existing options into the view model.
    /// </summary>
    public void Load(TcpServiceOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _useUdp = options.UseUdp;
        _mode = options.Mode;
        OnPropertyChanged(nameof(UseUdp));
        OnPropertyChanged(nameof(Mode));
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
    public event Action<TcpServiceOptions>? Saved;

    /// <summary>
    /// Raised when navigation back is requested.
    /// </summary>
    public event Action? BackRequested;

    /// <summary>
    /// Indicates whether UDP should be used instead of TCP.
    /// </summary>
    public bool UseUdp
    {
        get => _useUdp;
        set { _useUdp = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Available service modes.
    /// </summary>
    public TcpServiceMode Mode
    {
        get => _mode;
        set { _mode = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Modes available for selection.
    /// </summary>
    public TcpServiceMode[] Modes { get; } = (TcpServiceMode[])Enum.GetValues(typeof(TcpServiceMode));

    private void Save()
    {
        Logger?.Log("TCP advanced options start", LogLevel.Debug);
        if (_options is null) throw new InvalidOperationException("Options not loaded");
        _options.UseUdp = UseUdp;
        _options.Mode = Mode;
        Logger?.Log("TCP advanced options finished", LogLevel.Debug);
        Saved?.Invoke(_options);
    }

    private void Back()
    {
        Logger?.Log("TCP advanced options back", LogLevel.Debug);
        BackRequested?.Invoke();
    }
}
