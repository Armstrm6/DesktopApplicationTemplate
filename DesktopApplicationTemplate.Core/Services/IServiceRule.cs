using System;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Provides common validation rules for service configuration.
/// </summary>
public interface IServiceRule
{
    /// <summary>
    /// Validates that the specified value is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <returns>An error message if invalid; otherwise, <c>null</c>.</returns>
    string? ValidateRequired(string? value, string fieldName);

    /// <summary>
    /// Validates that the port lies within the inclusive range 1-65535.
    /// </summary>
    /// <param name="port">Port to validate.</param>
    /// <returns>An error message if invalid; otherwise, <c>null</c>.</returns>
    string? ValidatePort(int port);
}

