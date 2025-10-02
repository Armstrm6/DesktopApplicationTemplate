using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.Services.Common;

/// <summary>
/// Searches for files using <see cref="Directory.EnumerateFiles"/> with simple in-memory caching.
/// </summary>
public class FileSearchService : LoggedServiceBase<FileSearchService>, IFileSearchService
{
    private readonly ConcurrentDictionary<(string Directory, string Pattern), string[]> _cache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSearchService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public FileSearchService(ILogger<FileSearchService> logger) : base(logger)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetFilesAsync(string directoryPath, string searchPattern, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Value cannot be null or empty.", nameof(directoryPath));
        if (searchPattern is null)
            throw new ArgumentNullException(nameof(searchPattern));

        var key = (directoryPath, searchPattern);
        if (_cache.TryGetValue(key, out var cached))
        {
            Logger.LogDebug("Cache hit for {Directory} with pattern {Pattern}", directoryPath, searchPattern);
            return cached;
        }

        Logger.LogInformation("Enumerating files in {Directory} with pattern {Pattern}", directoryPath, searchPattern);
        var files = await Task.Run(() => Directory.EnumerateFiles(directoryPath, searchPattern).ToArray(), cancellationToken).ConfigureAwait(false);
        _cache[key] = files;
        return files;
    }
}
