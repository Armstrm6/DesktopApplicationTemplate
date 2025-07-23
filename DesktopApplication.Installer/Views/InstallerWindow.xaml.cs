using System.Windows;
using DesktopApplication.Installer.ViewModels;

namespace DesktopApplication.Installer.Views
{
    public partial class InstallerWindow : Window
    {
        private readonly InstallerWindowViewModel _vm = new();

        public InstallerWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            _vm.InstallRequested += OnInstallRequested;
        }

        private async void OnInstallRequested(string path, bool firewall, bool startup)
        {
            var progressVm = new ProgressWindowViewModel(path, firewall, startup);
            var progressWindow = new ProgressWindow { DataContext = progressVm, Owner = this };
            progressVm.Completed += () => progressWindow.Close();
            progressWindow.Show();
            await progressVm.StartAsync();
        }
    }
}
