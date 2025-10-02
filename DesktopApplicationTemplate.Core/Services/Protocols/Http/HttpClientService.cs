using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services.Protocols;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Http;

/// <summary>
/// Default implementation of <see cref="IHttpClientService"/> backed by <see cref="HttpClient"/>.
/// </summary>
public sealed class HttpClientService : IHttpClientService, IProtocolService
{
    private readonly IProtocolLogger? _logger;
    private string _name = nameof(HttpClientService);
    private bool _isRunning;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientService"/> class.
    /// </summary>
    public HttpClientService(IProtocolLogger? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => _name;

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _isRunning = true;
        _logger?.LogInformation(this, "HTTP client service started");
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _isRunning = false;
        _logger?.LogInformation(this, "HTTP client service stopped");
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpExecutionResult> ExecuteAsync(HttpExecutionRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        _name = request.RequestUri?.ToString() ?? nameof(HttpClientService);

        if (request.RequestUri is null)
        {
            throw new ArgumentException("A request URI must be supplied.", nameof(request));
        }

        _logger?.LogInformation(this, $"Preparing {request.Method} request to {request.RequestUri}");

        using var client = CreateClient(request);
        using var message = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value ?? string.Empty);
        }

        if (request.Body is not null && !IsBodylessMethod(request.Method))
        {
            message.Content = new StringContent(request.Body, Encoding.UTF8, request.ContentType);
            _logger?.LogInformation(this, $"HTTP request body prepared ({request.Body.Length} chars)");
        }

        try
        {
            _logger?.LogInformation(this, "Sending HTTP request");
            var response = await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogInformation(this, $"HTTP request completed with status {(int)response.StatusCode}");
            return new HttpExecutionResult(response.StatusCode, body);
        }
        catch (HttpRequestException httpEx)
        {
            _logger?.LogError(this, httpEx, "HTTP request failed");
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(this, ex, "Unexpected HTTP error");
            throw;
        }
        finally
        {
            _logger?.LogInformation(this, "HTTP request execution finished");
        }
    }

    private static HttpClient CreateClient(HttpExecutionRequest request)
    {
        if (request.MessageHandler is null)
        {
            return new HttpClient();
        }

        return new HttpClient(request.MessageHandler, disposeHandler: false);
    }

    private static bool IsBodylessMethod(HttpMethod method)
    {
        return HttpMethod.Get.Equals(method) || HttpMethod.Delete.Equals(method);
    }
}
