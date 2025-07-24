using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class FileObserverViewModel : ViewModelBase
    {
        public ObservableCollection<FileObserver> Observers { get; } = new();
        private FileObserver? _selectedObserver;
        public FileObserver SelectedObserver
        {
            get => _selectedObserver!;
            set { _selectedObserver = value; OnPropertyChanged(); LoadObserverData(); }
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

        public FileObserverViewModel()
        {
            AddObserverCommand = new RelayCommand(AddObserver);
            RemoveObserverCommand = new RelayCommand(RemoveObserver);
            BrowseCommand = new RelayCommand(BrowseFilePath);
            SaveCommand = new RelayCommand(Save);

            Observers.Add(new FileObserver { Name = "Observer1" });
        }

        private void LoadObserverData()
        {
            if (SelectedObserver == null) return;

            FilePath = SelectedObserver.FilePath;
            Contents = SelectedObserver.Contents;
            ImageNames = SelectedObserver.ImageNames;
            SendAllImages = SelectedObserver.SendAll;
            SendFirstXEnabled = SelectedObserver.SendFirstX;
            SendXCount = SelectedObserver.XCount.ToString();
            SendTcpCommandEnabled = SelectedObserver.SendTcp;
            TcpCommand = SelectedObserver.TcpString;

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
            if (SelectedObserver != null)
            {
                Observers.Remove(SelectedObserver);
                SelectedObserver = null;
            }
        }

        private void BrowseFilePath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
            }
        }

        private void Save() => SaveConfirmationHelper.Show();

        // OnPropertyChanged inherited from ViewModelBase
    }

    public class FileObserver
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Contents { get; set; }
        public string ImageNames { get; set; }
        public bool SendAll { get; set; }
        public bool SendFirstX { get; set; }
        public int XCount { get; set; } = 10;
        public bool SendTcp { get; set; }
        public string TcpString { get; set; }
    }
}
