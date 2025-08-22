using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for TcpServiceMessagesView.xaml
    /// </summary>
    public partial class TcpServiceMessagesView : Page
    {
        public TcpServiceMessagesView(TcpServiceMessagesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
