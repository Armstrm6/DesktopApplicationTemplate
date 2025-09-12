using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels.Ftp;

/// <summary>
/// View model for creating an FTP server.
/// </summary>
public class FtpServerCreateViewModel : ServiceCreateViewModelBase<FtpServerOptions>
{
    private int _port = 21;
    private string _rootPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpServerCreateViewModel"/> class.
    /// </summary>
    public FtpServerCreateViewModel(IServiceRule rule, IServiceScreen<FtpServerOptions> screen, ILoggingService? logger = null)
        : base(rule, screen, logger)
    {
    }

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
    protected override void ApplyOptions(FtpServerOptions options)
    {
        options.Port = Port;
        options.RootPath = RootPath;
    }
}
