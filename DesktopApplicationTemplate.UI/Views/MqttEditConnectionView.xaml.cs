using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

/// <summary>
/// Interaction logic for editing MQTT connection settings.
/// </summary>
public partial class MqttEditConnectionView : Window
{
    public MqttEditConnectionView(MqttEditConnectionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += (_, _) => Close();
    }
}
