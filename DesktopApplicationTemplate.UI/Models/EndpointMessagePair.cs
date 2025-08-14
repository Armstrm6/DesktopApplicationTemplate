using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopApplicationTemplate.UI.Models
{
    /// <summary>
    /// Represents a pair of endpoint and message for MQTT publishing.
    /// </summary>
    public class EndpointMessagePair : INotifyPropertyChanged
    {
        private string _endpoint = string.Empty;
        /// <summary>
        /// MQTT topic endpoint.
        /// </summary>
        public string Endpoint
        {
            get => _endpoint;
            set
            {
                _endpoint = value;
                OnPropertyChanged();
            }
        }

        private string _message = string.Empty;
        /// <summary>
        /// Payload message to send to the endpoint.
        /// </summary>
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
