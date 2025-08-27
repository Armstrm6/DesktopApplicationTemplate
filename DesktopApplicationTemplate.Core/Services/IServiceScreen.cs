using System;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Provides shared screen-composition operations for service create/edit workflows.
/// </summary>
/// <typeparam name="TOptions">Type of options managed by the screen.</typeparam>
public interface IServiceScreen<TOptions>
{
    /// <summary>
    /// Raised when the user requests to save the service.
    /// </summary>
    event Action<string, TOptions> Saved;

    /// <summary>
    /// Raised when the user cancels the operation.
    /// </summary>
    event Action Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    event Action<TOptions> AdvancedConfigRequested;

    /// <summary>
    /// Notifies the screen that the service should be saved.
    /// </summary>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="options">Current options.</param>
    void Save(string serviceName, TOptions options);

    /// <summary>
    /// Notifies the screen that the operation was cancelled.
    /// </summary>
    void Cancel();

    /// <summary>
    /// Requests that the advanced configuration view be displayed.
    /// </summary>
    /// <param name="options">Current options.</param>
    void OpenAdvanced(TOptions options);
}

