using System.Windows.Controls;
using DesktopApplicationTemplate.Services.Hid.UI.ViewModels.Hid;

namespace DesktopApplicationTemplate.Services.Hid.UI.Views.Hid
{
    /// <summary>
    /// Interaction logic for HidViews.xaml
    /// </summary>
    public partial class HidViews : Page
    {
        private readonly HidViewModel _viewModel;

        public HidViews(HidViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
        }
    }
}
