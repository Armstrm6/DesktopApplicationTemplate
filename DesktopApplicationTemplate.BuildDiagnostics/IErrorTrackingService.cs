using System;

namespace DesktopApplicationTemplate.BuildDiagnostics;

public interface IErrorTrackingService
{
    void RecordBuildError(string errorCode, string filePath, string description);

    void RecordRuntimeException(Exception exception, string? filePath);
}
