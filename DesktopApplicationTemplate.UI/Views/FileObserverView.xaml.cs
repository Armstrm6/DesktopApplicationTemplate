using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class FileObserverView : Page
    {
        private readonly FileObserverViewModel _viewModel;

        public FileObserverView(FileObserverViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            Loaded += FileObserverView_Loaded;
        }

        private void FileObserverView_Loaded(object sender, RoutedEventArgs e)
        {
            // Optional: logic on load (e.g., preload a directory, start watch service, etc.)
            // _viewModel.Initialize(); 
        }
    }
}
