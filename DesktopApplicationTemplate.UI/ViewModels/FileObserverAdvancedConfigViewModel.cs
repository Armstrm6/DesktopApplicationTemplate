using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced File Observer configuration.
/// </summary>
public class FileObserverAdvancedConfigViewModel : AdvancedConfigViewModelBase<FileObserverServiceOptions>
{
    private readonly FileObserverServiceOptions _options;
    private string _imageNames;
    private bool _sendAll;
    private bool _sendFirstX;
    private int _xCount;
    private bool _sendTcp;
    private string _tcpCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverAdvancedConfigViewModel"/> class.
    /// </summary>
    public FileObserverAdvancedConfigViewModel(FileObserverServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _imageNames = options.ImageNames;
        _sendAll = options.SendAllImages;
        _sendFirstX = options.SendFirstX;
        _xCount = options.XCount;
        _sendTcp = options.SendTcpCommand;
        _tcpCommand = options.TcpCommand;
    }

    /// <summary>
    /// Comma-separated list of image names.
    /// </summary>
    public string ImageNames
    {
        get => _imageNames;
        set { _imageNames = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether to send all images.
    /// </summary>
    public bool SendAllImages
    {
        get => _sendAll;
        set { _sendAll = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether to send only the first X images.
    /// </summary>
    public bool SendFirstX
    {
        get => _sendFirstX;
        set { _sendFirstX = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Number of images to send when <see cref="SendFirstX"/> is true.
    /// </summary>
    public int XCount
    {
        get => _xCount;
        set { _xCount = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether to send a TCP command.
    /// </summary>
    public bool SendTcpCommand
    {
        get => _sendTcp;
        set { _sendTcp = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// TCP command to send.
    /// </summary>
    public string TcpCommand
    {
        get => _tcpCommand;
        set { _tcpCommand = value; OnPropertyChanged(); }
    }

    protected override FileObserverServiceOptions OnSave()
    {
        Logger?.Log("FileObserver advanced options start", LogLevel.Debug);
        _options.ImageNames = ImageNames;
        _options.SendAllImages = SendAllImages;
        _options.SendFirstX = SendFirstX;
        _options.XCount = XCount;
        _options.SendTcpCommand = SendTcpCommand;
        _options.TcpCommand = TcpCommand;
        Logger?.Log("FileObserver advanced options finished", LogLevel.Debug);
        return _options;
    }

    protected override void OnBack()
    {
        Logger?.Log("FileObserver advanced options back", LogLevel.Debug);
    }
}
