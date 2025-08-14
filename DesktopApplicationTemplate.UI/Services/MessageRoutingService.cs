using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Provides a simple mechanism for routing messages between services.
    /// </summary>
    public class MessageRoutingService
    {
        private readonly ILoggingService _logger;

        public MessageRoutingService(ILoggingService logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Routes a message from a source service to a destination service.
        /// </summary>
        /// <param name="source">The originating service.</param>
        /// <param name="destination">The target service.</param>
        /// <param name="message">The message to route.</param>
        public void Route(string source, string destination, string message)
        {
            _logger.Log($"Routing message from {source} to {destination}", LogLevel.Debug);
            MessageForwarder.Forward(destination, message);
            _logger.Log("Message routed", LogLevel.Debug);
        }
    }
}
