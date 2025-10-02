using System;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;

/// <summary>
/// Monitors files and directories for the file observer protocol.
/// </summary>
public interface IFileObserverService : IProtocolService
{
    /// <summary>
    /// Raised when the observed file changes.
    /// </summary>
    event EventHandler<FileObserverChangedEventArgs>? FileChanged;

    /// <summary>
    /// Configures the observer for the specified service.
    /// </summary>
    Task ConfigureAsync(string observerName, FileObserverServiceOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current snapshot of the observed file.
    /// </summary>
    Task<FileObserverSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
