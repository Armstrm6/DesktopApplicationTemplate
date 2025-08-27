using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Service.Services;

/// <summary>
/// Default implementation of <see cref="IServiceRule"/>.
/// </summary>
public class ServiceRule : IServiceRule
{
    /// <inheritdoc />
    public string? ValidateRequired(string? value, string fieldName)
        => string.IsNullOrWhiteSpace(value) ? $"{fieldName} is required" : null;

    /// <inheritdoc />
    public string? ValidatePort(int port)
        => port < 1 || port > 65535 ? "Port must be between 1 and 65535" : null;
}

