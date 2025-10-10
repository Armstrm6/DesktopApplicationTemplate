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
        public const int MaxRows = 100;
        private readonly Dictionary<string, LinkedList<ServiceMessageRow>> _messagesByService = new(StringComparer.OrdinalIgnoreCase);
        private readonly IServiceLookup _serviceLookup;
        private string _activeServiceKey = string.Empty;

        /// <summary>Collection of service message rows.</summary>
        public ObservableCollection<ServiceMessageRow> Messages { get; } = new();

        public ServiceMessageTableViewModel(IServiceLookup? serviceLookup = null)
        {
            _serviceLookup = serviceLookup ?? NullServiceLookup.Instance;
        }

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
            var timestamp = DateTime.Now;

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
                Timestamp = timestamp
            };

            rows.AddFirst(row);
            while (rows.Count > MaxRows)
            {
                rows.RemoveLast();
            }

            if (_serviceLookup.TryGetService(serviceType, resolvedServiceName, out var service) && service is not null)
            {
                service.RecordMessageHistory(incoming, outgoing, destination, timestamp);
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

        /// <summary>
        /// Loads persisted messages for a service so previously recorded exchanges reappear in the UI.
        /// </summary>
        public void LoadMessages(ServiceType serviceType, string serviceName, IEnumerable<ServiceMessageHistoryEntry> entries)
        {
            var resolvedServiceName = serviceName?.Trim() ?? string.Empty;
            var key = BuildKey(serviceType, resolvedServiceName);
            if (!_messagesByService.TryGetValue(key, out var rows))
            {
                rows = new LinkedList<ServiceMessageRow>();
                _messagesByService[key] = rows;
            }
            else
            {
                rows.Clear();
            }

            if (entries is not null)
            {
                foreach (var entry in entries)
                {
                    if (entry is null)
                    {
                        continue;
                    }

                    var row = new ServiceMessageRow
                    {
                        IncomingMessage = MessageDisplayFormatter.FormatForService(entry.IncomingMessage, true, serviceType, resolvedServiceName),
                        OutgoingMessage = MessageDisplayFormatter.FormatForService(entry.OutgoingMessage, false, serviceType, resolvedServiceName),
                        Destination = entry.Destination ?? string.Empty,
                        Timestamp = entry.Timestamp
                    };

                    rows.AddLast(row);
                }
            }

            if (string.Equals(key, _activeServiceKey, StringComparison.OrdinalIgnoreCase))
            {
                Messages.Clear();
                foreach (var row in rows)
                {
                    Messages.Add(row);
                }
            }
        }

        private static string BuildKey(ServiceType serviceType, string serviceName)
        {
            return $"{serviceType}:{serviceName}";
        }

        private sealed class NullServiceLookup : IServiceLookup
        {
            internal static readonly NullServiceLookup Instance = new();

            private NullServiceLookup()
            {
            }

            public bool TryGetService(ServiceType type, string name, out ServiceListModel? service)
            {
                service = null;
                return false;
            }

            public IEnumerable<ServiceListModel> FindByDisplayName(string name)
            {
                return Array.Empty<ServiceListModel>();
            }
        }
    }
}
