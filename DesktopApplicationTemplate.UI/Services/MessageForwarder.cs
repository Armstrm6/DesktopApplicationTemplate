using System.Linq;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Provides simple forwarding of HID messages to another service.
    /// </summary>
    public static class MessageForwarder
    {
        /// <summary>
        /// Optional action used for testing to intercept forwarding logic.
        /// </summary>
        public static System.Action<string, string>? ForwardAction { get; set; }

        public static void Forward(string serviceName, string message)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                return;

            if (ForwardAction != null)
            {
                ForwardAction(serviceName, message);
                return;
            }

            var mainVm = App.AppHost.Services.GetService(typeof(MainViewModel)) as MainViewModel;
            var target = mainVm?.Services.FirstOrDefault(s => s.DisplayName == serviceName);
            target?.AddLog($"[HID Forward] {message}");
        }
    }
}
