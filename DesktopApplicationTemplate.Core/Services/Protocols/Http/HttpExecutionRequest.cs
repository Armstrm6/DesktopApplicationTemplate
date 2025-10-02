using System;
using System.Collections.Generic;
using System.Net.Http;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Http;

/// <summary>
/// Describes an HTTP request to be executed by <see cref="IHttpClientService"/>.
/// </summary>
public sealed class HttpExecutionRequest
{
    /// <summary>
    /// Gets or sets the request URI.
    /// </summary>
    public Uri RequestUri { get; set; } = new("http://localhost");

    /// <summary>
    /// Gets or sets the HTTP method to use when sending the request.
    /// </summary>
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    /// Gets or sets the request body payload.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the content type applied when a body is supplied.
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Gets or sets the optional message handler to use for the request.
    /// </summary>
    public HttpMessageHandler? MessageHandler { get; set; }

    /// <summary>
    /// Gets the headers applied to the request.
    /// </summary>
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
