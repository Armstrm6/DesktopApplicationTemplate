using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Http;

/// <summary>
/// Executes HTTP requests independent of any UI concerns.
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    /// Sends the supplied request and returns the response payload.
    /// </summary>
    /// <param name="request">The request descriptor to execute.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The HTTP execution result.</returns>
    Task<HttpExecutionResult> ExecuteAsync(HttpExecutionRequest request, CancellationToken cancellationToken = default);
}
