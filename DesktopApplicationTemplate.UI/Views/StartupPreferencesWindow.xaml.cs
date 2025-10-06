using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class StartupPreferencesWindow : Window
    {
        public StartupPreferencesWindow(StartupPreferencesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += OnRequestClose;
            Closed += (_, _) => viewModel.RequestClose -= OnRequestClose;
        }

        private void OnRequestClose(object? sender, bool accepted)
        {
            DialogResult = accepted;
            Close();
        }
    }
}

