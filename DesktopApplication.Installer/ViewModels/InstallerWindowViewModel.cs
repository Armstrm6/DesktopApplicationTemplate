using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplication.Installer.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplication.Installer.ViewModels
{
    internal class InstallerWindowViewModel : INotifyPropertyChanged
    {
        private readonly ILoggingService? _logger;
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

        public InstallerWindowViewModel(ILoggingService? logger = null)
        {
            _logger = logger;
            BrowseCommand = new RelayCommand(Browse);
            InstallCommand = new RelayCommand(Install);
            _logger?.Log("Installer window initialized", LogLevel.Debug);
        }

        private void Browse()
        {
            _logger?.Log("Browse for install directory invoked", LogLevel.Debug);
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InstallPath = dialog.SelectedPath;
                _logger?.Log($"Install path selected: {InstallPath}", LogLevel.Debug);
            }
        }

        private void Install()
        {
            _logger?.Log("Install command triggered", LogLevel.Debug);
            InstallRequested?.Invoke(InstallPath, AddFirewallRule, RunOnStartup);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
