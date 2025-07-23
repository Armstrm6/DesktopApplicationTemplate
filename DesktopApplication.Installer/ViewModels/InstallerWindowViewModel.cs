using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplication.Installer.Helpers;

namespace DesktopApplication.Installer.ViewModels
{
    internal class InstallerWindowViewModel : INotifyPropertyChanged
    {
        private string _installPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        private bool _addFirewallRule;
        private bool _runOnStartup;

        public string InstallPath
        {
            get => _installPath;
            set { _installPath = value; OnPropertyChanged(); }
        }

        public bool AddFirewallRule
        {
            get => _addFirewallRule;
            set { _addFirewallRule = value; OnPropertyChanged(); }
        }

        public bool RunOnStartup
        {
            get => _runOnStartup;
            set { _runOnStartup = value; OnPropertyChanged(); }
        }

        public ICommand BrowseCommand { get; }
        public ICommand InstallCommand { get; }

        public event Action<string, bool, bool>? InstallRequested;

        public InstallerWindowViewModel()
        {
            BrowseCommand = new RelayCommand(Browse);
            InstallCommand = new RelayCommand(Install);
        }

        private void Browse()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InstallPath = dialog.SelectedPath;
            }
        }

        private void Install()
        {
            InstallRequested?.Invoke(InstallPath, AddFirewallRule, RunOnStartup);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
