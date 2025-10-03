using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.UI.Helpers;
using Microsoft.VisualStudio.Threading;

namespace DesktopApplicationTemplate.UI.ViewModels.FileObserver;

public class FileObserverViewModel : ViewModelBase
{
    private static readonly JoinableTaskFactory _jtf = new(new JoinableTaskContext());

    public ObservableCollection<FileObserver> Observers { get; } = new();

    private FileObserver? _selectedObserver;
    public FileObserver? SelectedObserver
    {
        get => _selectedObserver;
        set
        {
            if (_selectedObserver == value)
            {
                return;
            }

            _selectedObserver = value;
            OnPropertyChanged();
            _ = LoadObserverDataAsync();
        }
    }

    private string _filePath = string.Empty;
    public string FilePath
    {
        get => _filePath;
        set { _filePath = value; OnPropertyChanged(); }
    }

    private string _contents = string.Empty;
    public string Contents
    {
        get => _contents;
        set { _contents = value; OnPropertyChanged(); }
    }

    private string _imageNames = string.Empty;
    public string ImageNames
    {
        get => _imageNames;
        set { _imageNames = value; OnPropertyChanged(); }
    }

    private bool _sendAllImages;
    public bool SendAllImages
    {
        get => _sendAllImages;
        set { _sendAllImages = value; OnPropertyChanged(); }
    }

    private bool _sendFirstXEnabled;
    public bool SendFirstXEnabled
    {
        get => _sendFirstXEnabled;
        set { _sendFirstXEnabled = value; OnPropertyChanged(); }
    }

    private string _sendXCount = "10";
    public string SendXCount
    {
        get => _sendXCount;
        set { _sendXCount = value; OnPropertyChanged(); }
    }

    private bool _sendTcpCommandEnabled;
    public bool SendTcpCommandEnabled
    {
        get => _sendTcpCommandEnabled;
        set { _sendTcpCommandEnabled = value; OnPropertyChanged(); }
    }

    private string _tcpCommand = string.Empty;
    public string TcpCommand
    {
        get => _tcpCommand;
        set { _tcpCommand = value; OnPropertyChanged(); }
    }

    public ICommand AddObserverCommand { get; }
    public ICommand RemoveObserverCommand { get; }
    public ICommand BrowseCommand { get; }
    public ICommand SaveCommand { get; }

    private readonly SaveConfirmationHelper _saveHelper;
    private readonly IFileSearchService _fileSearchService;
    private readonly IFileObserverService _fileObserverService;

    public FileObserverViewModel(
        SaveConfirmationHelper saveHelper,
        IFileSearchService fileSearchService,
        IFileObserverService fileObserverService)
    {
        _saveHelper = saveHelper ?? throw new ArgumentNullException(nameof(saveHelper));
        _fileSearchService = fileSearchService ?? throw new ArgumentNullException(nameof(fileSearchService));
        _fileObserverService = fileObserverService ?? throw new ArgumentNullException(nameof(fileObserverService));

        AddObserverCommand = new RelayCommand(AddObserver);
        RemoveObserverCommand = new RelayCommand(RemoveObserver);
        BrowseCommand = new AsyncRelayCommand(BrowseFilePathAsync);
        SaveCommand = new RelayCommand(Save);

        _fileObserverService.FileChanged += OnFileChanged;
        Observers.Add(new FileObserver { Name = "Observer1" });
    }

    private async Task LoadObserverDataAsync()
    {
        var observer = SelectedObserver;
        if (observer is null)
        {
            ResetFields();
            await _fileObserverService.StopAsync().ConfigureAwait(false);
            return;
        }

        FilePath = observer.FilePath;
        Contents = observer.Contents;
        ImageNames = observer.ImageNames;
        SendAllImages = observer.SendAll;
        SendFirstXEnabled = observer.SendFirstX;
        SendXCount = observer.XCount.ToString();
        SendTcpCommandEnabled = observer.SendTcp;
        TcpCommand = observer.TcpString;

        OnPropertyChanged(null);
        await ConfigureObserverServiceAsync(observer).ConfigureAwait(false);
    }

    private void ResetFields()
    {
        FilePath = string.Empty;
        Contents = string.Empty;
        ImageNames = string.Empty;
        SendAllImages = false;
        SendFirstXEnabled = false;
        SendXCount = "10";
        SendTcpCommandEnabled = false;
        TcpCommand = string.Empty;
        OnPropertyChanged(null);
    }

    private void AddObserver()
    {
        var observer = new FileObserver { Name = $"Observer{Observers.Count + 1}" };
        Observers.Add(observer);
        SelectedObserver = observer;
    }

    private void RemoveObserver()
    {
        if (SelectedObserver is null)
        {
            return;
        }

        Observers.Remove(SelectedObserver);
        SelectedObserver = null;
        _ = _fileObserverService.StopAsync();
    }

    private async Task BrowseFilePathAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        if (dialog.ShowDialog() == true)
        {
            FilePath = dialog.FileName;

            var directory = System.IO.Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                try
                {
                    var files = await _fileSearchService
                        .GetFilesAsync(directory, "*", CancellationToken.None)
                        .ConfigureAwait(false);
                    ImageNames = string.Join(",", files.Select(System.IO.Path.GetFileName));
                }
                catch
                {
                    ImageNames = string.Empty;
                }
            }

            if (SelectedObserver is not null)
            {
                SelectedObserver.FilePath = FilePath;
                await ConfigureObserverServiceAsync(SelectedObserver).ConfigureAwait(false);
            }
        }
    }

    private void Save() => _saveHelper.Show();

    private async Task ConfigureObserverServiceAsync(FileObserver observer)
    {
        var options = new FileObserverServiceOptions
        {
            FilePath = observer.FilePath,
            ImageNames = observer.ImageNames,
            SendAllImages = observer.SendAll,
            SendFirstX = observer.SendFirstX,
            XCount = observer.XCount,
            SendTcpCommand = observer.SendTcp,
            TcpCommand = observer.TcpString
        };

        await _fileObserverService.ConfigureAsync(observer.Name, options).ConfigureAwait(false);
        await _fileObserverService.StartAsync().ConfigureAwait(false);
        var snapshot = await _fileObserverService.GetSnapshotAsync().ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(snapshot.Contents))
        {
            observer.Contents = snapshot.Contents;
            Contents = snapshot.Contents;
        }

        if (snapshot.ImageNames.Length > 0 && string.IsNullOrWhiteSpace(observer.ImageNames))
        {
            observer.ImageNames = string.Join(",", snapshot.ImageNames);
            ImageNames = observer.ImageNames;
        }
    }

    private void OnFileChanged(object? sender, FileObserverChangedEventArgs e) => _ = HandleFileChangedAsync(e);

    private async Task HandleFileChangedAsync(FileObserverChangedEventArgs e)
    {
        var application = Application.Current;
        if (application?.Dispatcher is null)
        {
            ApplyFileContents(e.Contents);
            return;
        }

        if (!application.Dispatcher.CheckAccess())
        {
            await _jtf.SwitchToMainThreadAsync();
        }

        ApplyFileContents(e.Contents);
    }

    private void ApplyFileContents(string contents)
    {
        if (SelectedObserver is not null)
        {
            SelectedObserver.Contents = contents;
        }

        Contents = contents;
    }
}

public class FileObserver
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Contents { get; set; } = string.Empty;
    public string ImageNames { get; set; } = string.Empty;
    public bool SendAll { get; set; }
    public bool SendFirstX { get; set; }
    public int XCount { get; set; } = 10;
    public bool SendTcp { get; set; }
    public string TcpString { get; set; } = string.Empty;
}
