using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced FTP server options.
/// </summary>
public class FtpServerAdvancedConfigViewModel : AdvancedConfigViewModelBase<FtpServerOptions>
{
    private readonly FtpServerOptions _options;
    private bool _allowAnonymous;
    private string? _username;
    private string? _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpServerAdvancedConfigViewModel"/> class.
    /// </summary>
    public FtpServerAdvancedConfigViewModel(FtpServerOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _allowAnonymous = options.AllowAnonymous;
        _username = options.Username;
        _password = options.Password;
    }

    /// <summary>
    /// Allow anonymous connections.
    /// </summary>
    public bool AllowAnonymous
    {
        get => _allowAnonymous;
        set
        {
            _allowAnonymous = value;
            if (!_allowAnonymous)
            {
                ValidateCredentials();
            }
            else
            {
                ClearErrors(nameof(Username));
                ClearErrors(nameof(Password));
            }
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Username for authenticated access.
    /// </summary>
    public string? Username
    {
        get => _username;
        set
        {
            _username = value;
            if (!AllowAnonymous && string.IsNullOrWhiteSpace(value))
                AddError(nameof(Username), "Username is required");
            else
                ClearErrors(nameof(Username));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Password for authenticated access.
    /// </summary>
    public string? Password
    {
        get => _password;
        set
        {
            _password = value;
            if (!AllowAnonymous && string.IsNullOrWhiteSpace(value))
                AddError(nameof(Password), "Password is required");
            else
                ClearErrors(nameof(Password));
            OnPropertyChanged();
        }
    }

    protected override FtpServerOptions? OnSave()
    {
        ValidateCredentials();
        if (HasErrors)
            return null;
        Logger?.Log("FTP advanced options start", LogLevel.Debug);
        _options.AllowAnonymous = AllowAnonymous;
        _options.Username = string.IsNullOrWhiteSpace(Username) ? null : Username;
        _options.Password = string.IsNullOrWhiteSpace(Password) ? null : Password;
        Logger?.Log("FTP advanced options finished", LogLevel.Debug);
        return _options;
    }

    protected override void OnBack()
    {
        Logger?.Log("FTP advanced options back", LogLevel.Debug);
    }

    private void ValidateCredentials()
    {
        if (!AllowAnonymous && string.IsNullOrWhiteSpace(Username))
            AddError(nameof(Username), "Username is required");
        else
            ClearErrors(nameof(Username));

        if (!AllowAnonymous && string.IsNullOrWhiteSpace(Password))
            AddError(nameof(Password), "Password is required");
        else
            ClearErrors(nameof(Password));
    }
}
