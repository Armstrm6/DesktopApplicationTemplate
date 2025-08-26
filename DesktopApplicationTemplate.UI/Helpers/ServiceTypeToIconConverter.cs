using System;
using System.Globalization;
using System.Windows.Data;

namespace DesktopApplicationTemplate.UI.Helpers
{
    /// <summary>
    /// Converts a service type string into a representative emoji icon.
    /// </summary>
    public class ServiceTypeToIconConverter : IValueConverter
    {
        /// <summary>
        /// Maps a service type to an emoji icon used in the UI selection grid.
        /// </summary>
        /// <param name="value">The service type string.</param>
        /// <param name="targetType">The expected target type (unused).</param>
        /// <param name="parameter">Optional parameter (unused).</param>
        /// <param name="culture">Culture information (unused).</param>
        /// <returns>An emoji string representing the service type.</returns>
        /// <exception cref="ArgumentException">Thrown when value is not a string.</exception>
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string type)
                throw new ArgumentException("Value must be a service type string", nameof(value));

            return type switch
            {
                "HID" => "🕹️",
                "TCP" => "🔌",
                "HTTP" => "🌐",
                "File Observer" => "📁",
                "Heartbeat" => "❤️",
                "CSV Creator" => "📄",
                "SCP" => "🔒",
                "MQTT" => "📡",
                "FTP" => "📤",
                _ => "❓"
            };
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
