using System;
using System.Collections.ObjectModel;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// View model providing a tabular collection of service messages.
    /// </summary>
    public class ServiceMessageTableViewModel : ViewModelBase
    {
        private const int MaxRows = 5;

        /// <summary>Collection of service message rows.</summary>
        public ObservableCollection<ServiceMessageRow> Messages { get; } = new();

        /// <summary>
        /// Adds a new message row and inserts it at the top to maintain
        /// descending timestamp order.
        /// </summary>
        public void AddMessage(ServiceType serviceType, string serviceName, string incoming, string outgoing, string destination)
        {
            var resolvedServiceName = serviceName ?? string.Empty;
            var incomingDisplay = MessageDisplayFormatter.FormatForService(incoming, true, serviceType, resolvedServiceName);
            var outgoingDisplay = MessageDisplayFormatter.FormatForService(outgoing, false, serviceType, resolvedServiceName);

            Messages.Insert(0, new ServiceMessageRow
            {
                IncomingMessage = incomingDisplay,
                OutgoingMessage = outgoingDisplay,
                Destination = destination ?? string.Empty,
                Timestamp = DateTime.Now
            });

            if (Messages.Count > MaxRows)
                Messages.RemoveAt(Messages.Count - 1);
        }
    }
}
