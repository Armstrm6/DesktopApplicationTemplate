using System.Collections.Generic;

namespace DesktopApplicationTemplate.Core.Services
{
    /// <summary>
    /// Provides helpers for managing CSV output associated with services.
    /// </summary>
    public interface ICsvService
    {
        /// <summary>
        /// Ensures that CSV columns exist for the supplied service name.
        /// </summary>
        /// <param name="serviceName">Display name of the service.</param>
        void EnsureColumnsForService(string serviceName);

        /// <summary>
        /// Removes CSV columns that were previously associated with the supplied service name.
        /// </summary>
        /// <param name="serviceName">Display name of the service.</param>
        void RemoveColumnsForService(string serviceName);

        /// <summary>
        /// Records a log entry for the supplied service into the CSV output.
        /// </summary>
        /// <param name="serviceName">Display name of the service.</param>
        /// <param name="message">Message to record.</param>
        void RecordLog(string serviceName, string message);

        /// <summary>
        /// Appends a row of values to the CSV output.
        /// </summary>
        /// <param name="values">Row values.</param>
        void AppendRow(IEnumerable<string?> values);
    }
}
