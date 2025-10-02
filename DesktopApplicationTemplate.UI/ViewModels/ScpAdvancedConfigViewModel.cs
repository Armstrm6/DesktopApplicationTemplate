using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced SCP configuration.
/// </summary>
public class ScpAdvancedConfigViewModel : AdvancedConfigViewModelBase<ScpServiceOptions>
{
    private ScpServiceOptions? _options;
    private string _localPath = string.Empty;
    private string _remotePath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScpAdvancedConfigViewModel"/> class.
    /// </summary>
    public ScpAdvancedConfigViewModel(ILoggingService? logger = null)
        : base(logger)
    {
    }

    /// <summary>
    /// Loads existing options into the view model.
    /// </summary>
    /// <param name="options">Options to edit.</param>
    public void Load(ScpServiceOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _localPath = options.LocalPath;
        _remotePath = options.RemotePath;
        OnPropertyChanged(nameof(LocalPath));
        OnPropertyChanged(nameof(RemotePath));
    }

    /// <summary>
    /// Local file path to upload.
    /// </summary>
    public string LocalPath
    {
        get => _localPath;
        set { _localPath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Remote destination path.
    /// </summary>
    public string RemotePath
    {
        get => _remotePath;
        set { _remotePath = value; OnPropertyChanged(); }
    }

    protected override ScpServiceOptions? OnSave()
    {
        Logger?.Log("SCP advanced options start", LogLevel.Debug);
        if (_options is null) throw new InvalidOperationException("Options not loaded");
        _options.LocalPath = LocalPath;
        _options.RemotePath = RemotePath;
        Logger?.Log("SCP advanced options finished", LogLevel.Debug);
        return _options;
    }

    protected override void OnBack()
    {
        Logger?.Log("SCP advanced options back", LogLevel.Debug);
    }
}
