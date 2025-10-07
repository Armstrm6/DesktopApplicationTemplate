using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols;
using DesktopApplicationTemplate.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Tcp;

/// <summary>
/// Provides Roslyn-backed script execution for TCP services.
/// </summary>
public sealed class TcpRuntime : ITcpRuntime
{
    private readonly IMessageRoutingService _routingService;
    private readonly IProtocolLogger? _logger;
    private TcpRuntimeContext? _context;
    private bool _isRunning;
    private string _name = nameof(TcpRuntime);

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpRuntime"/> class.
    /// </summary>
    public TcpRuntime(IMessageRoutingService routingService, IProtocolLogger? logger = null)
    {
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
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
        _logger?.LogInformation(this, "TCP runtime started");
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _isRunning = false;
        _logger?.LogInformation(this, "TCP runtime stopped");
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TcpRuntimeState> InitializeAsync(TcpRuntimeContext context, CancellationToken cancellationToken = default)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _name = $"{context.ServiceType}.{context.ServiceName}";

        var options = context.Options ?? throw new ArgumentNullException(nameof(context.Options));
        var script = string.IsNullOrWhiteSpace(options.Script) ? context.DefaultScript : options.Script;
        var testMessage = ResolveInitialMessage(context, out var isInputMessage);

        _logger?.LogInformation(this, "Initializing TCP script runtime");

        var output = await ExecuteScriptInternalAsync(script, testMessage, cancellationToken).ConfigureAwait(false);

        options.Script = script;
        options.InputMessage = testMessage;
        if (!isInputMessage)
        {
            options.LastTestMessage = testMessage;
        }
        options.OutputMessage = output;

        _routingService.UpdateMessage(context.ServiceType, context.ServiceName, testMessage, MessageRoutingDirection.Input);
        _routingService.UpdateMessage(context.ServiceType, context.ServiceName, output, MessageRoutingDirection.Output);

        return new TcpRuntimeState(script, testMessage, output);
    }

    /// <inheritdoc />
    public async Task<TcpRuntimeState> ExecuteAsync(TcpRuntimeExecutionRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var context = request.Context ?? throw new ArgumentException("Context is required", nameof(request));
        _context = context;
        _name = $"{context.ServiceType}.{context.ServiceName}";

        _logger?.LogInformation(this, "Executing TCP script");
        var script = request.Script ?? string.Empty;
        var message = request.TestMessage ?? string.Empty;
        var output = await ExecuteScriptInternalAsync(script, message, cancellationToken).ConfigureAwait(false);

        context.Options.Script = script;
        context.Options.InputMessage = message;
        if (!request.IsLiveInput)
        {
            context.Options.LastTestMessage = message;
        }
        context.Options.OutputMessage = output;

        _routingService.UpdateMessage(context.ServiceType, context.ServiceName, message, MessageRoutingDirection.Input);
        _routingService.UpdateMessage(context.ServiceType, context.ServiceName, output, MessageRoutingDirection.Output);

        return new TcpRuntimeState(script, message, output);
    }

    private string ResolveInitialMessage(TcpRuntimeContext context, out bool isInputMessage)
    {
        var options = context.Options;
        var serviceName = context.ServiceName;

        if (_routingService.TryGetMessage(context.ServiceType, serviceName, MessageRoutingDirection.Input, out var routed))
        {
            isInputMessage = true;
            var resolved = routed ?? string.Empty;
            options.InputMessage = resolved;
            return resolved;
        }

        if (!string.IsNullOrWhiteSpace(options.LastTestMessage))
        {
            isInputMessage = false;
            return options.LastTestMessage;
        }

        isInputMessage = false;
        return $"{serviceName}-PEAK-123456789";
    }

    private async Task<string> ExecuteScriptInternalAsync(string script, string message, CancellationToken cancellationToken)
    {
        var referencingService = _context?.ServiceName;
        var rewrittenScript = MessageRoutingScriptTransformer.InjectRoutingLiterals(script, _routingService, referencingService);
        var globals = new TcpScriptGlobals { Message = message ?? string.Empty };
        var code = rewrittenScript + "\nreturn Process(Message);";
        var compiled = CSharpScript.Create<string>(code, ScriptOptions.Default, typeof(TcpScriptGlobals));
        var diagnostics = compiled.Compile();

        if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            var joined = string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString()));
            _logger?.LogWarning(this, $"TCP script compile failed: {joined}");
            return joined;
        }

        try
        {
            var result = await compiled.RunAsync(globals, cancellationToken: cancellationToken).ConfigureAwait(false);
            var output = result.ReturnValue ?? string.Empty;
            _logger?.LogInformation(this, "TCP script executed successfully");
            return output;
        }
        catch (Exception ex)
        {
            _logger?.LogError(this, ex, "TCP script execution failed");
            return ex.ToString();
        }
    }
}
