using System.Collections.Generic;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Csv;

/// <summary>
/// Defines CSV logging functionality that operates on configuration and output abstractions.
/// </summary>
public interface ICsvService
{
    /// <summary>
    /// Ensures the configuration contains the columns required for the specified service.
    /// </summary>
    /// <param name="configuration">The CSV configuration.</param>
    /// <param name="serviceName">The service name.</param>
    /// <returns><c>true</c> when the configuration was modified; otherwise <c>false</c>.</returns>
    bool EnsureColumnsForService(CsvConfiguration configuration, string serviceName);

    /// <summary>
    /// Removes columns associated with the specified service.
    /// </summary>
    /// <param name="configuration">The CSV configuration.</param>
    /// <param name="state">The current CSV runtime state.</param>
    /// <param name="serviceName">The service name.</param>
    /// <returns><c>true</c> when the configuration was modified; otherwise <c>false</c>.</returns>
    bool RemoveColumnsForService(CsvConfiguration configuration, CsvServiceState state, string serviceName);

    /// <summary>
    /// Records a log message to the CSV output.
    /// </summary>
    /// <param name="configuration">The CSV configuration.</param>
    /// <param name="state">The current CSV runtime state.</param>
    /// <param name="output">The output abstraction.</param>
    /// <param name="serviceName">The originating service name.</param>
    /// <param name="message">The log message to record.</param>
    void RecordLog(CsvConfiguration configuration, CsvServiceState state, ICsvOutput output, string serviceName, string message);

    /// <summary>
    /// Appends an arbitrary row to the CSV output.
    /// </summary>
    /// <param name="configuration">The CSV configuration.</param>
    /// <param name="state">The current CSV runtime state.</param>
    /// <param name="output">The output abstraction.</param>
    /// <param name="values">The values to write.</param>
    void AppendRow(CsvConfiguration configuration, CsvServiceState state, ICsvOutput output, IEnumerable<string?> values);
}
