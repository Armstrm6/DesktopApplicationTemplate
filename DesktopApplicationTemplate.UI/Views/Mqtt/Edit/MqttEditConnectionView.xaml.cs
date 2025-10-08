using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Edit;

namespace DesktopApplicationTemplate.UI.Views.Mqtt.Edit;

public partial class MqttEditConnectionView : Page
{
    public MqttEditConnectionView()
    {
        InitializeComponent();
    }

    public void Initialize(MqttEditConnectionViewModel viewModel)
    {
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }
}
