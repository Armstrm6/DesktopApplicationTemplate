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
            _viewModel.Load();
            _ = _networkViewModel.LoadAsync();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Save();
            Services.ThemeManager.ApplyTheme(_viewModel.DarkTheme);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigateBack();
        }

        private void Logo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigateBack();
        }

        public void NavigateBack()
        {
            if (_viewModel.HasUnsavedChanges)
            {
                var res = MessageBox.Show("Save changes?", "Settings", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Cancel)
                    return;
                if (res == MessageBoxResult.Yes)
                {
                    _viewModel.Save();
                    Services.ThemeManager.ApplyTheme(_viewModel.DarkTheme);
                }
            }

            var mainWindow = Window.GetWindow(this) as MainView;
            var mainViewModel = mainWindow?.DataContext as MainViewModel;
            if (mainWindow != null && mainViewModel != null)
            {
                mainWindow.ContentFrame.Content = new HomePage { DataContext = mainViewModel };
            }
        }
    }
}
