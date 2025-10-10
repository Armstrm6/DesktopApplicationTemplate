using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.VisualStudio.Threading;

namespace DesktopApplicationTemplate.UI.Helpers;

/// <summary>
/// Resolves routed service attribute expressions and tracks referenced attributes for change notifications.
/// </summary>
public sealed class ServiceAttributeExpressionBinder
{
    private static readonly Regex TokenRegex = new("\\{([A-Za-z0-9_]+)\\.([A-Za-z0-9_]+)\\}", RegexOptions.Compiled);

    private readonly IMessageRoutingService _routingService;
    private readonly Action<string> _onResolved;
    private readonly Action<string?>? _onError;
    private string? _referencingServiceName;
    private readonly JoinableTaskFactory? _joinableTaskFactory;

    private readonly object _sync = new();
    private List<AttributeReference> _references = new();

    private string _expression = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceAttributeExpressionBinder"/> class.
    /// </summary>
    /// <param name="routingService">Routing service used to resolve expressions.</param>
    /// <param name="onResolved">Callback invoked with the resolved value.</param>
    /// <param name="onError">Optional callback invoked with an error message when referenced attributes are missing.</param>
    /// <param name="referencingServiceName">Optional service name that owns the expression.</param>
    /// <param name="joinableTaskFactory">Optional task factory used to marshal callbacks to the UI thread.</param>
    public ServiceAttributeExpressionBinder(
        IMessageRoutingService routingService,
        Action<string> onResolved,
        Action<string?>? onError = null,
        string? referencingServiceName = null,
        JoinableTaskFactory? joinableTaskFactory = null)
    {
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
        _onResolved = onResolved ?? throw new ArgumentNullException(nameof(onResolved));
        _onError = onError;
        _referencingServiceName = referencingServiceName;
        _joinableTaskFactory = joinableTaskFactory ?? App.UiThreadTaskFactory;

        WeakEventManager<IMessageRoutingService, ServiceAttributeChangedEventArgs>.AddHandler(
            _routingService,
            nameof(IMessageRoutingService.AttributeChanged),
            OnAttributeChanged);
    }

    /// <summary>
    /// Gets or sets the attribute expression to resolve.
    /// </summary>
    public string Expression
    {
        get => _expression;
        set
        {
            var normalized = value ?? string.Empty;
            if (string.Equals(_expression, normalized, StringComparison.Ordinal))
            {
                return;
            }

            _expression = normalized;
            lock (_sync)
            {
                _references = ParseReferences(_expression);
            }

            Resolve();
        }
    }

    /// <summary>
    /// Forces the binder to re-evaluate the current expression.
    /// </summary>
    public void Refresh() => Resolve();

    /// <summary>
    /// Gets or sets the service name that owns the expression.
    /// </summary>
    public string? ReferencingServiceName
    {
        get => _referencingServiceName;
        set
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            if (string.Equals(_referencingServiceName, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _referencingServiceName = normalized;
            Resolve();
        }
    }

    private void Resolve()
    {
        string resolved;
        string? error = null;

        try
        {
            resolved = string.IsNullOrEmpty(_expression)
                ? string.Empty
                : _routingService.ResolveTokens(_expression, _referencingServiceName);

            var missing = GetMissingReferences();
            if (missing.Count > 0)
            {
                error = $"Missing attributes: {string.Join(", ", missing)}";
            }
        }
        catch (Exception ex)
        {
            resolved = string.Empty;
            error = ex.Message;
        }

        PostCallbacks(resolved, error);
    }

    private void PostCallbacks(string resolved, string? error)
    {
        void Execute()
        {
            try
            {
                _onResolved(resolved);
                _onError?.Invoke(error);
            }
            catch (Exception ex)
            {
                _onError?.Invoke(ex.Message);
            }
        }

        if (_joinableTaskFactory is { } factory)
        {
            var task = factory.RunAsync(async () =>
            {
                await factory.SwitchToMainThreadAsync();
                Execute();
            }).Task;

            ObserveTask(task);
        }
        else
        {
            Execute();
        }
    }

    private List<string> GetMissingReferences()
    {
        List<AttributeReference> references;
        lock (_sync)
        {
            references = _references;
        }

        if (references.Count == 0)
        {
            return new List<string>();
        }

        var missing = new List<string>();
        foreach (var reference in references)
        {
            bool exists = reference.Direction.HasValue
                ? _routingService.TryGetMessage(reference.Service, reference.Direction.Value, out _)
                : _routingService.TryGetAttribute(reference.Service, reference.Attribute, out _);

            if (!exists)
            {
                missing.Add($"{reference.Service}.{reference.Attribute}");
            }
        }

        return missing;
    }

    private static void ObserveTask(Task task)
    {
        if (task is null)
        {
            return;
        }

        _ = task.ContinueWith(
            t => _ = t.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private static List<AttributeReference> ParseReferences(string expression)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return new List<AttributeReference>();
        }

        var matches = TokenRegex.Matches(expression);
        if (matches.Count == 0)
        {
            return new List<AttributeReference>();
        }

        var references = new List<AttributeReference>(matches.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in matches)
        {
            if (!match.Success || match.Groups.Count < 3)
            {
                continue;
            }

            var service = match.Groups[1].Value.Trim();
            var attribute = match.Groups[2].Value.Trim();
            if (service.Length == 0 || attribute.Length == 0)
            {
                continue;
            }

            var normalizedAttribute = NormalizeAttribute(attribute, out var direction);
            var key = string.Concat(service, '|', normalizedAttribute);
            if (seen.Add(key))
            {
                references.Add(new AttributeReference(service, normalizedAttribute, direction));
            }
        }

        return references;
    }

    private static string NormalizeAttribute(string attribute, out MessageRoutingDirection? direction)
    {
        if (attribute.Equals("InputMessage", StringComparison.OrdinalIgnoreCase) ||
            attribute.Equals("LastInputMessage", StringComparison.OrdinalIgnoreCase))
        {
            direction = MessageRoutingDirection.Input;
            return "InputMessage";
        }

        if (attribute.Equals("OutputMessage", StringComparison.OrdinalIgnoreCase) ||
            attribute.Equals("LastOutputMessage", StringComparison.OrdinalIgnoreCase))
        {
            direction = MessageRoutingDirection.Output;
            return "OutputMessage";
        }

        direction = null;
        return attribute;
    }

    private void OnAttributeChanged(object? sender, ServiceAttributeChangedEventArgs e)
    {
        List<AttributeReference> references;
        lock (_sync)
        {
            if (_references.Count == 0)
            {
                return;
            }

            references = _references;
        }

        foreach (var reference in references)
        {
            if (string.Equals(reference.Service, e.ServiceName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(reference.Attribute, e.AttributeName, StringComparison.OrdinalIgnoreCase))
            {
                Resolve();
                break;
            }
        }
    }

    private readonly record struct AttributeReference(string Service, string Attribute, MessageRoutingDirection? Direction);
}
