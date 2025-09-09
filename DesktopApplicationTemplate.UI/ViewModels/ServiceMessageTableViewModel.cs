using System;
using System.Collections.ObjectModel;
using DesktopApplicationTemplate.UI.Models;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// View model providing a tabular collection of service messages.
    /// </summary>
    public class ServiceMessageTableViewModel : ViewModelBase
    {
        /// <summary>Collection of service message rows.</summary>
        public ObservableCollection<ServiceMessageRow> Messages { get; } = new();

        /// <summary>
        /// Adds a new message row and inserts it at the top to maintain
        /// descending timestamp order.
        /// </summary>
        public void AddMessage(string incoming, string outgoing, string destination)
        {
            Messages.Insert(0, new ServiceMessageRow
            {
                IncomingMessage = incoming ?? string.Empty,
                OutgoingMessage = outgoing ?? string.Empty,
                Destination = destination ?? string.Empty,
                Timestamp = DateTime.Now
            });
        }
    }
}
