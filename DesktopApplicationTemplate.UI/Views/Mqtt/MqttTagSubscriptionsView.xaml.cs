using DesktopApplicationTemplate.UI.ViewModels;
using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Views.Mqtt;

/// <summary>
/// Interaction logic for MqttTagSubscriptionsView.xaml
/// </summary>
public partial class MqttTagSubscriptionsView : Page, IServiceLogHost
{
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttTagSubscriptionsView"/> class.
    /// </summary>
    public MqttTagSubscriptionsView(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeComponent();
    }

    /// <summary>
    /// Applies the supplied view model to the page.
    /// </summary>
    /// <param name="service">The service that owns the view model.</param>
    /// <param name="viewModel">The view model to use as the data context.</param>
    public void Initialize(ServiceListModel service, MqttTagSubscriptionsViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(viewModel);

        viewModel.Logger = _logger;
        DataContext = viewModel;
        SetServiceContext(service);
    }

    /// <inheritdoc />
    public void SetServiceContext(ServiceListModel service)
    {
        LogView.DataContext = new ServiceLogViewModel(service.Type, service.Logs);
    }
}
