using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class SettingsPage : Page
    {
        private readonly SettingsViewModel _viewModel;
        private readonly NetworkConfigurationViewModel _networkViewModel;

        public SettingsPage(SettingsViewModel settingsViewModel, NetworkConfigurationViewModel networkViewModel)
        {
            InitializeComponent();
            _viewModel = settingsViewModel;
            _networkViewModel = networkViewModel;
            DataContext = _viewModel;
            NetworkConfigGrid.DataContext = _networkViewModel;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            await _viewModel.LoadAsync().ConfigureAwait(false);
            await _networkViewModel.LoadAsync().ConfigureAwait(false);
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            await SaveAndApplyThemeAsync().ConfigureAwait(false);
        }

        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            await NavigateBackAsync().ConfigureAwait(false);
        }

        private async void BackText_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            await NavigateBackAsync().ConfigureAwait(false);
        }

        public async Task NavigateBackAsync()
        {
            if (_viewModel.HasUnsavedChanges)
            {
                var res = MessageBox.Show("Save changes?", "Settings", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Cancel)
                    return;
                if (res == MessageBoxResult.Yes)
                {
                    await SaveAndApplyThemeAsync();
                }
            }

            if (Window.GetWindow(this) is not MainView mainWindow)
            {
                return;
            }

            mainWindow.ShowHome();
        }

        private async Task SaveAndApplyThemeAsync()
        {
            await _viewModel.SaveAsync().ConfigureAwait(false);
            await App.UiThreadTaskFactory.SwitchToMainThreadAsync();
            Services.ThemeManager.ApplyTheme(_viewModel.DarkTheme);
        }
    }
}
