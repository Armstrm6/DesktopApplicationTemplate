using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, LinkedList<ServiceMessageRow>> _messagesByService = new(StringComparer.OrdinalIgnoreCase);
        private string _activeServiceKey = string.Empty;

        /// <summary>Collection of service message rows.</summary>
        public ObservableCollection<ServiceMessageRow> Messages { get; } = new();

        /// <summary>
        /// Sets the active service whose messages should be surfaced to the UI.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceName">The service name.</param>
        public void SetActiveService(ServiceType serviceType, string serviceName)
        {
            _activeServiceKey = BuildKey(serviceType, serviceName?.Trim() ?? string.Empty);
            Messages.Clear();

            if (_messagesByService.TryGetValue(_activeServiceKey, out var rows))
            {
                foreach (var row in rows)
                {
                    Messages.Add(row);
                }
            }
        }

        /// <summary>
        /// Adds a new message row and inserts it at the top to maintain
        /// descending timestamp order.
        /// </summary>
        public void AddMessage(ServiceType serviceType, string serviceName, string incoming, string outgoing, string destination)
        {
            var resolvedServiceName = serviceName?.Trim() ?? string.Empty;
            var incomingDisplay = MessageDisplayFormatter.FormatForService(incoming, true, serviceType, resolvedServiceName);
            var outgoingDisplay = MessageDisplayFormatter.FormatForService(outgoing, false, serviceType, resolvedServiceName);

            var key = BuildKey(serviceType, resolvedServiceName);
            if (!_messagesByService.TryGetValue(key, out var rows))
            {
                rows = new LinkedList<ServiceMessageRow>();
                _messagesByService[key] = rows;
            }

            var row = new ServiceMessageRow
            {
                IncomingMessage = incomingDisplay,
                OutgoingMessage = outgoingDisplay,
                Destination = destination ?? string.Empty,
                Timestamp = DateTime.Now
            };

            rows.AddFirst(row);
            while (rows.Count > MaxRows)
            {
                rows.RemoveLast();
            }

            if (string.Equals(key, _activeServiceKey, StringComparison.OrdinalIgnoreCase))
            {
                Messages.Insert(0, row);
                if (Messages.Count > MaxRows)
                {
                    Messages.RemoveAt(Messages.Count - 1);
                }
            }
        }

        private static string BuildKey(ServiceType serviceType, string serviceName)
        {
            return $"{serviceType}:{serviceName}";
        }
    }
}
