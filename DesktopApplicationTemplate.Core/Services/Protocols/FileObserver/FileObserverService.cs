using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services.Protocols;

namespace DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;

/// <summary>
/// Default implementation of <see cref="IFileObserverService"/> using <see cref="FileSystemWatcher"/>.
/// </summary>
public sealed class FileObserverService : IFileObserverService
{
    private readonly IProtocolLogger? _logger;
    private FileObserverServiceOptions? _options;
    private FileSystemWatcher? _watcher;
    private string _name = nameof(FileObserverService);
    private bool _isRunning;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverService"/> class.
    /// </summary>
    public FileObserverService(IProtocolLogger? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public event EventHandler<FileObserverChangedEventArgs>? FileChanged;

    /// <inheritdoc />
    public string Name => _name;

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    public async Task ConfigureAsync(string observerName, FileObserverServiceOptions options, CancellationToken cancellationToken = default)
    {
        _name = observerName ?? nameof(FileObserverService);
        _options = options ?? throw new ArgumentNullException(nameof(options));
        await StopAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogInformation(this, $"Configured to watch {options.FilePath}");
    }

    /// <inheritdoc />
    public async Task<FileObserverSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await Task.Run(ReadSnapshot, cancellationToken).ConfigureAwait(false);
        return snapshot;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_options is null)
        {
            throw new InvalidOperationException("ConfigureAsync must be called before StartAsync.");
        }

        if (_isRunning)
        {
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(_options.FilePath))
        {
            _logger?.LogWarning(this, "No file path configured for file observer");
            return Task.CompletedTask;
        }

        var directory = Path.GetDirectoryName(_options.FilePath);
        var fileName = Path.GetFileName(_options.FilePath);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
        {
            _logger?.LogWarning(this, $"Invalid file observer path {_options.FilePath}");
            return Task.CompletedTask;
        }

        _watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Renamed += OnFileChanged;
        _watcher.EnableRaisingEvents = true;
        _isRunning = true;
        _logger?.LogInformation(this, $"Started watching {_options.FilePath}");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_watcher is not null)
        {
            _logger?.LogInformation(this, "Stopping file observer");
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnFileChanged;
            _watcher.Renamed -= OnFileChanged;
            _watcher.Dispose();
            _watcher = null;
        }

        _isRunning = false;
        return Task.CompletedTask;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            var snapshot = ReadSnapshot();
            FileChanged?.Invoke(this, new FileObserverChangedEventArgs(e.FullPath, snapshot.Contents));
            _logger?.LogInformation(this, $"File observer detected change on {e.FullPath}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(this, ex, "Error reading observed file");
        }
    }

    private FileObserverSnapshot ReadSnapshot()
    {
        if (_options is null)
        {
            return new FileObserverSnapshot(string.Empty, Array.Empty<string>());
        }

        string contents = string.Empty;
        try
        {
            if (!string.IsNullOrWhiteSpace(_options.FilePath) && File.Exists(_options.FilePath))
            {
                contents = File.ReadAllText(_options.FilePath);
            }
        }
        catch (IOException ex)
        {
            _logger?.LogWarning(this, $"Unable to read {_options.FilePath}: {ex.Message}");
        }

        var imageNames = (_options.ImageNames ?? string.Empty)
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        return new FileObserverSnapshot(contents, imageNames);
    }
}
