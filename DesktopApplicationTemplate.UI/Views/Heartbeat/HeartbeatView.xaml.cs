using DesktopApplicationTemplate.UI.ViewModels.Heartbeat;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.UI.Views.Heartbeat
{
    public partial class HeartbeatView : Page
    {
        public HeartbeatView(HeartbeatViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Help_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var help = new AsciiHelpWindow();
            help.ShowDialog();
        }
    }
}
