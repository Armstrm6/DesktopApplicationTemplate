using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class SettingsPage : Page
    {
        private readonly SettingsViewModel _viewModel;
        private readonly NetworkConfigurationViewModel _networkViewModel;

        public ICommand SaveCommand { get; }
        public ICommand NavigateBackCommand { get; }

        public SettingsPage(SettingsViewModel settingsViewModel, NetworkConfigurationViewModel networkViewModel)
        {
            InitializeComponent();
            _viewModel = settingsViewModel;
            _networkViewModel = networkViewModel;
            DataContext = _viewModel;
            NetworkConfigGrid.DataContext = _networkViewModel;
            SaveCommand = new AsyncRelayCommand(SaveAndApplyThemeAsync);
            NavigateBackCommand = new AsyncRelayCommand(NavigateBackAsync);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            _ = LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
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
                    await SaveAndApplyThemeAsync().ConfigureAwait(false);
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
