using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class FileObserverViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FileObserver> Observers { get; } = new();
        private FileObserver _selectedObserver;
        public FileObserver SelectedObserver
        {
            get => _selectedObserver;
            set { _selectedObserver = value; OnPropertyChanged(); LoadObserverData(); }
        }

        public string FilePath { get; set; }
        public string Contents { get; set; }
        public string ImageNames { get; set; }
        public bool SendAllImages { get; set; }
        public bool SendFirstXEnabled { get; set; }
        public string SendXCount { get; set; } = "10";
        public bool SendTcpCommandEnabled { get; set; }
        public string TcpCommand { get; set; }

        public ICommand AddObserverCommand { get; }
        public ICommand RemoveObserverCommand { get; }
        public ICommand BrowseCommand { get; }

        public FileObserverViewModel()
        {
            AddObserverCommand = new RelayCommand(AddObserver);
            RemoveObserverCommand = new RelayCommand(RemoveObserver);
            BrowseCommand = new RelayCommand(BrowseFilePath);

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
            Observers.Add(new FileObserver { Name = $"Observer{Observers.Count + 1}" });
        }

        private void RemoveObserver()
        {
            if (SelectedObserver != null)
                Observers.Remove(SelectedObserver);
        }

        private void BrowseFilePath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
