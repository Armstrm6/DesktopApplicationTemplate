using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;

namespace DesktopApplicationTemplate.UI.ViewModels.Scp.Create;

/// <summary>
/// View model for creating a new SCP service.
/// </summary>
public class ScpCreateServiceViewModel : ServiceCreateViewModelBase<ScpServiceOptions>
{
    private string _host = string.Empty;
    private string _port = "22";
    private string _username = string.Empty;
    private string _password = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScpCreateServiceViewModel"/> class.
    /// </summary>
    public ScpCreateServiceViewModel(IServiceRule rule, ILoggingService? logger = null)
        : base(rule, logger: logger)
    {
    }

    /// <summary>
    /// SCP host name.
    /// </summary>
    public string Host
    {
        get => _host;
        set
        {
            _host = value;
            var error = Rule.ValidateRequired(value, "Host");
            if (error is not null)
            {
                AddError(nameof(Host), error);
                Logger?.Log(error, LogLevel.Warning);
            }
            else
            {
                ClearErrors(nameof(Host));
            }
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// SCP port number.
    /// </summary>
    public string Port
    {
        get => _port;
        set
        {
            _port = value;
            if (int.TryParse(value, out var port))
            {
                var error = Rule.ValidatePort(port);
                if (error is not null)
                {
                    AddError(nameof(Port), error);
                    Logger?.Log(error, LogLevel.Warning);
                }
                else
                {
                    ClearErrors(nameof(Port));
                }
            }
            else
            {
                var error = "Port must be a number";
                AddError(nameof(Port), error);
                Logger?.Log(error, LogLevel.Warning);
            }
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Username for authentication.
    /// </summary>
    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            var error = Rule.ValidateRequired(value, "Username");
            if (error is not null)
            {
                AddError(nameof(Username), error);
                Logger?.Log(error, LogLevel.Warning);
            }
            else
            {
                ClearErrors(nameof(Username));
            }
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Password for authentication.
    /// </summary>
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            var error = Rule.ValidateRequired(value, "Password");
            if (error is not null)
            {
                AddError(nameof(Password), error);
                Logger?.Log(error, LogLevel.Warning);
            }
            else
            {
                ClearErrors(nameof(Password));
            }
            OnPropertyChanged();
        }
    }

    /// <inheritdoc />
    protected override void ApplyOptions(ScpServiceOptions options)
    {
        options.Host = Host;
        if (int.TryParse(Port, out var port))
            options.Port = port;
        options.Username = Username;
        options.Password = Password;
    }
}
