using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Views;

/// <summary>
/// Interaction logic for MqttTagSubscriptionsView.xaml
/// </summary>
public partial class MqttTagSubscriptionsView : Page, IServiceLogHost
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MqttTagSubscriptionsView"/> class.
    /// </summary>
    public MqttTagSubscriptionsView(MqttTagSubscriptionsViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        vm.Logger = logger;
        DataContext = vm;
    }

    /// <inheritdoc />
    public void SetServiceContext(ServiceListModel service)
    {
        LogView.DataContext = new ServiceLogViewModel(service.ServiceType, service.Logs);
    }
}
