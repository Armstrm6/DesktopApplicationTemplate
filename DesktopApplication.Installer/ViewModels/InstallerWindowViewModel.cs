using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplication.Installer.Helpers;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplication.Installer.ViewModels
{
    internal class InstallerWindowViewModel : INotifyPropertyChanged
    {
        private readonly ILoggingService? _logger;
        private string _installPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        private bool _addFirewallRule;
        private bool _runOnStartup;
        private bool _isInstalled;

        public string InstallPath
        {
            get => _installPath;
            set
            {
                _installPath = value;
                OnPropertyChanged();
                UpdateIsInstalled();
            }
        }

        public bool IsInstalled
        {
            get => _isInstalled;
            private set { _isInstalled = value; OnPropertyChanged(); }
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
        public ICommand UninstallCommand { get; }
        public ICommand CheckUpdatesCommand { get; }

        public event Action<string, bool, bool>? InstallRequested;
        public event Action<string>? UninstallRequested;
        public event Action? CheckUpdatesRequested;

        public InstallerWindowViewModel(ILoggingService? logger = null)
        {
            _logger = logger;
            BrowseCommand = new RelayCommand(Browse);
            InstallCommand = new RelayCommand(Install);
            UninstallCommand = new RelayCommand(Uninstall);
            CheckUpdatesCommand = new RelayCommand(CheckForUpdates);
            UpdateIsInstalled();
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

        private void Uninstall()
        {
            _logger?.Log("Uninstall command triggered", LogLevel.Debug);
            UninstallRequested?.Invoke(InstallPath);
        }

        private void CheckForUpdates()
        {
            _logger?.Log("Check for updates command triggered", LogLevel.Debug);
            CheckUpdatesRequested?.Invoke();
        }

        private void UpdateIsInstalled()
        {
            IsInstalled = File.Exists(Path.Combine(InstallPath, "install_log.txt"));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
