using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced CSV creator configuration.
/// </summary>
public class CsvAdvancedConfigViewModel : AdvancedConfigViewModelBase<CsvServiceOptions>
{
    private readonly CsvServiceOptions _options;
    private string _delimiter;
    private bool _includeHeaders;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvAdvancedConfigViewModel"/> class.
    /// </summary>
    public CsvAdvancedConfigViewModel(CsvServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _delimiter = options.Delimiter;
        _includeHeaders = options.IncludeHeaders;
    }

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

    protected override CsvServiceOptions OnSave()
    {
        Logger?.Log("CSV advanced options start", LogLevel.Debug);
        _options.Delimiter = Delimiter;
        _options.IncludeHeaders = IncludeHeaders;
        Logger?.Log("CSV advanced options finished", LogLevel.Debug);
        return _options;
    }

    protected override void OnBack()
    {
        Logger?.Log("CSV advanced options back", LogLevel.Debug);
    }
}
