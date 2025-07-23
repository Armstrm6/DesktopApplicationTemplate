using DesktopApplicationTemplate.UI.ViewModels;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class HeartbeatView : Page
    {
        public HeartbeatView(HeartbeatViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
