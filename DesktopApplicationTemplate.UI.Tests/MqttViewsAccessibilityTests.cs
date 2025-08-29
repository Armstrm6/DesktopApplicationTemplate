using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using MQTTnet.Client;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class MqttViewsAccessibilityTests
    {
        [WindowsFact]
        public void MqttCreateServiceView_ExposesNamedButtons()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var view = new MqttCreateServiceView(new MqttCreateServiceViewModel(), new Mock<ILoggingService>().Object);
                var create = (Button)view.FindName("CreateButton");
                Assert.Equal("Create MQTT Service", AutomationProperties.GetName(create));
                ConsoleTestLogger.LogPass();
            });
        }

        [WindowsFact]
        public void MqttEditConnectionView_ExposesUpdateButton()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var options = Options.Create(new MqttServiceOptions { Host = "h", Port = 1, ClientId = "c" });
                var service = new MqttService(options, new Mock<IMessageRoutingService>().Object, new Mock<ILoggingService>().Object);
                var vm = new MqttEditConnectionViewModel(service, options, new Mock<ILoggingService>().Object);
                var view = new MqttEditConnectionView(vm);
                var update = (Button)view.FindName("UpdateButton");
                Assert.Equal("Update Connection", AutomationProperties.GetName(update));
                ConsoleTestLogger.LogPass();
            });
        }

        [WindowsFact]
        public void MqttTagSubscriptionsView_ExposesSendButton()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var options = Options.Create(new MqttServiceOptions { Host = "h", Port = 1, ClientId = "c" });
                var service = new MqttService(options, new Mock<IMessageRoutingService>().Object, new Mock<ILoggingService>().Object);
                var vm = new MqttTagSubscriptionsViewModel(service) { Logger = new Mock<ILoggingService>().Object };
                var view = new MqttTagSubscriptionsView(vm, new Mock<ILoggingService>().Object);
                var send = (Button)view.FindName("SendButton");
                Assert.Equal("Send Test Message", AutomationProperties.GetName(send));
                ConsoleTestLogger.LogPass();
            });
        }
    }
}
