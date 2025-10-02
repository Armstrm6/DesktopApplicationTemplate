using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating an FTP server.
/// </summary>
public class FtpServerCreateViewModel : ServiceCreateViewModelBase<FtpServerOptions>
{
    private readonly IServiceScreen<FtpServerOptions> _screen;
    private int _port = 21;
    private string _rootPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpServerCreateViewModel"/> class.
    /// </summary>
    public FtpServerCreateViewModel(IServiceRule rule, IServiceScreen<FtpServerOptions> screen, ILoggingService? logger = null)
        : base(rule, logger)
    {
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));

        _screen.ServiceSaved += (_, o) => RaiseServiceSaved(o);
        _screen.EditCancelled += () => RaiseEditCancelled();
        _screen.AdvancedConfigRequested += o => RaiseAdvancedConfigRequested(o);
    }

    /// <summary>
    /// Current advanced options.
    /// </summary>
    public FtpServerOptions Options { get; } = new();

    /// <summary>
    /// Port to listen on.
    /// </summary>
    public int Port
    {
        get => _port;
        set
        {
            _port = value;
            var error = Rule.ValidatePort(value);
            if (error is not null)
                AddError(nameof(Port), error);
            else
                ClearErrors(nameof(Port));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Root directory for the server.
    /// </summary>
    public string RootPath
    {
        get => _rootPath;
        set
        {
            _rootPath = value;
            var error = Rule.ValidateRequired(value, "Root path");
            if (error is not null)
                AddError(nameof(RootPath), error);
            else
                ClearErrors(nameof(RootPath));
            OnPropertyChanged();
        }
    }

    /// <inheritdoc />
    protected override void OnSave()
    {
        if (HasErrors)
            return;
        Options.Port = Port;
        Options.RootPath = RootPath;
        _screen.Save(ServiceName, Options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => _screen.Cancel();

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        Options.Port = Port;
        Options.RootPath = RootPath;
        _screen.OpenAdvanced(Options);
    }
}
