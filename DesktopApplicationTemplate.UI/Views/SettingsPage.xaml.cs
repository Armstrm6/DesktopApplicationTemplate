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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            ObserveTask(LoadAsync());
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ObserveTask(SaveAndApplyThemeAsync());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ObserveTask(NavigateBackAsync());
        }

        private void BackText_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ObserveTask(NavigateBackAsync());
        }

        private async Task LoadAsync()
        {
            await _viewModel.LoadAsync().ConfigureAwait(false);
            await _networkViewModel.LoadAsync().ConfigureAwait(false);
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

        private static void ObserveTask(Task? task)
        {
            if (task is null)
            {
                return;
            }

            _ = task.ContinueWith(
                t => _ = t.Exception,
                System.Threading.CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
    }
}
