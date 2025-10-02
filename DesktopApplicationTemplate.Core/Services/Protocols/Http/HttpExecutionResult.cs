using System.Net;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Http;

/// <summary>
/// Represents the outcome of executing an HTTP request.
/// </summary>
public sealed class HttpExecutionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpExecutionResult"/> class.
    /// </summary>
    public HttpExecutionResult(HttpStatusCode statusCode, string body)
    {
        StatusCode = statusCode;
        Body = body;
    }

    /// <summary>
    /// Gets the HTTP status code returned by the server.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the response payload returned by the server.
    /// </summary>
    public string Body { get; }
}
