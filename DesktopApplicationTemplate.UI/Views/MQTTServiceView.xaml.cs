using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class MQTTServiceView : Page
    {
        private readonly MqttServiceViewModel _viewModel;
        public MQTTServiceView(MqttServiceViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = vm;
            _viewModel.Logger = new LoggingService(LogBox, Dispatcher);
        }
    }
}
