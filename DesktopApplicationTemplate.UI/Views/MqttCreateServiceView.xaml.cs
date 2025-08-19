using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

public partial class MqttCreateServiceView : Page
{
    public MqttCreateServiceView(MqttCreateServiceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
