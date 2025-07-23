using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class SettingsPage : Page
    {
        private readonly SettingsViewModel _viewModel;
        public SettingsPage(SettingsViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = _viewModel;
            _viewModel.Load();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Save();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            HandleBack();
        }

        private void Logo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HandleBack();
        }

        private void HandleBack()
        {
            if (_viewModel.HasUnsavedChanges)
            {
                var res = MessageBox.Show("Save changes?", "Settings", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Cancel)
                    return;
                if (res == MessageBoxResult.Yes)
                    _viewModel.Save();
            }
            if (Parent is Frame frame)
                frame.Content = new HomePage { DataContext = App.AppHost.Services.GetService(typeof(MainViewModel)) };
        }
    }
}
