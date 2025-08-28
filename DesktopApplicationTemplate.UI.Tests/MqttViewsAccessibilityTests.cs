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
    [Collection("WpfTests")]
    public class MqttViewsAccessibilityTests
    {
        [WpfFact]
        public void MqttCreateServiceView_ExposesNamedButtons()
        {

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (Application.Current == null)
                        new DesktopApplicationTemplate.UI.App();
                    var view = new MqttCreateServiceView(new MqttCreateServiceViewModel(), new Mock<ILoggingService>().Object);
                    var create = (Button)view.FindName("CreateButton");
                    Assert.Equal("Create MQTT Service", AutomationProperties.GetName(create));
                }
                catch (Exception e) { ex = e; }
                finally { Application.Current?.Shutdown(); }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }

        [WpfFact]
        public void MqttEditConnectionView_ExposesUpdateButton()
        {

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (Application.Current == null)
                        new DesktopApplicationTemplate.UI.App();
                    var options = Options.Create(new MqttServiceOptions { Host = "h", Port = 1, ClientId = "c" });
                    var service = new MqttService(options, new Mock<IMessageRoutingService>().Object, new Mock<ILoggingService>().Object);
                    var vm = new MqttEditConnectionViewModel(service, options, new Mock<ILoggingService>().Object);
                    var view = new MqttEditConnectionView(vm);
                    var update = (Button)view.FindName("UpdateButton");
                    Assert.Equal("Update Connection", AutomationProperties.GetName(update));
                }
                catch (Exception e) { ex = e; }
                finally { Application.Current?.Shutdown(); }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }

        [WpfFact]
        public void MqttTagSubscriptionsView_ExposesSendButton()
        {

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (Application.Current == null)
                        new DesktopApplicationTemplate.UI.App();
                    var options = Options.Create(new MqttServiceOptions { Host = "h", Port = 1, ClientId = "c" });
                    var service = new MqttService(options, new Mock<IMessageRoutingService>().Object, new Mock<ILoggingService>().Object);
                    var vm = new MqttTagSubscriptionsViewModel(service) { Logger = new Mock<ILoggingService>().Object };
                    var view = new MqttTagSubscriptionsView(vm, new Mock<ILoggingService>().Object);
                    var send = (Button)view.FindName("SendButton");
                    Assert.Equal("Send Test Message", AutomationProperties.GetName(send));
                }
                catch (Exception e) { ex = e; }
                finally { Application.Current?.Shutdown(); }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }
    }
}
