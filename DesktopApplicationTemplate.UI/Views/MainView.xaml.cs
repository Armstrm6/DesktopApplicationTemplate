using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class MainView : Window
    {
        private readonly ILogger<MainView>? _logger;

        public MainView(MainViewModel viewModel, ILogger<MainView>? logger = null)
        {
            InitializeComponent();
            DataContext = viewModel;
            _logger = logger;
            _logger?.LogInformation("MainView initialized");
        }
    }
}
