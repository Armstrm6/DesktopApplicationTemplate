using System.Threading.Tasks;
using System.Windows;
using DesktopApplication.Installer.Services;
using DesktopApplication.Installer.ViewModels;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplication.Installer.Views
{
    public partial class InstallerWindow : Window
    {
        private readonly InstallerWindowViewModel _vm;
        private readonly IUninstallService _uninstallService;

        public InstallerWindow(IUninstallService? uninstallService = null, ILoggingService? logger = null)
        {
            InitializeComponent();
            _vm = new InstallerWindowViewModel(logger);
            DataContext = _vm;
            _uninstallService = uninstallService ?? new UninstallService(new ProcessManager(), logger);
            _vm.InstallRequested += OnInstallRequested;
            _vm.UninstallRequested += OnUninstallRequested;
            _vm.CheckUpdatesRequested += OnCheckUpdatesRequested;
        }

        private async void OnInstallRequested(string path, bool firewall, bool startup)
        {
            var progressVm = new ProgressWindowViewModel(path, firewall, startup);
            var progressWindow = new ProgressWindow { DataContext = progressVm, Owner = this };
            progressVm.Completed += () => progressWindow.Close();
            progressWindow.Show();
            await progressVm.StartAsync();
            _vm.InstallPath = path;
        }

        private async void OnUninstallRequested(string path)
        {
            await _uninstallService.UninstallAsync(path).ConfigureAwait(false);
            System.Windows.MessageBox.Show(this, "Uninstallation completed", "Uninstall", MessageBoxButton.OK, MessageBoxImage.Information);
            _vm.InstallPath = path;
        }

        private void OnCheckUpdatesRequested()
        {
            System.Windows.MessageBox.Show(this, "No updates available", "Updates", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
