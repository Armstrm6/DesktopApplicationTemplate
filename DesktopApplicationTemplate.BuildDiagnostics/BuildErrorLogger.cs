using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DesktopApplicationTemplate.BuildDiagnostics;

public sealed class BuildErrorLogger : Logger
{
    private IEventSource? _eventSource;
    private IErrorTrackingService? _errorTrackingService;

    public override void Initialize(IEventSource eventSource)
    {
        if (eventSource is null)
        {
            throw new ArgumentNullException(nameof(eventSource));
        }

        _eventSource = eventSource;
        _errorTrackingService = CreateErrorTrackingService(Parameters);
        _eventSource.ErrorRaised += OnErrorRaised;
    }

    public override void Shutdown()
    {
        if (_eventSource is not null)
        {
            _eventSource.ErrorRaised -= OnErrorRaised;
            _eventSource = null;
        }

        _errorTrackingService = null;
    }

    private void OnErrorRaised(object sender, BuildErrorEventArgs e)
    {
        if (_errorTrackingService is null)
        {
            return;
        }

        var description = string.IsNullOrWhiteSpace(e.Message) ? "Unspecified" : e.Message.Trim();
        var filePath = string.IsNullOrWhiteSpace(e.File) ? "Unknown" : e.File;
        var errorCode = string.IsNullOrWhiteSpace(e.Code) ? "UNKNOWN" : e.Code.Trim();

        _errorTrackingService.RecordBuildError(errorCode, filePath, description);
    }

    private static IErrorTrackingService CreateErrorTrackingService(string? parameters)
    {
        var rootDirectory = string.IsNullOrWhiteSpace(parameters) ? null : parameters.Trim();
        return new ExcelErrorTrackingService(rootDirectory);
    }
}
