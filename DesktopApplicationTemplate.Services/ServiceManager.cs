using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols;
using DesktopApplicationTemplate.Services.Protocols;
using Microsoft.Extensions.Logging;
using LogLevel = DesktopApplicationTemplate.Core.Services.LogLevel;

namespace DesktopApplicationTemplate.Services;

/// <summary>
/// Coordinates service descriptor runtimes and UI registrations.
/// </summary>
/// <typeparam name="TService">The UI service model type.</typeparam>
/// <typeparam name="TPage">The UI navigation target type.</typeparam>
public sealed class ServiceManager<TService, TPage> : IDisposable
{
    private readonly IServiceCatalog _serviceCatalog;
    private readonly IServiceRuntimeFactory _runtimeFactory;
    private readonly IServiceUiRegistry<TService, TPage> _uiRegistry;
    private readonly IReadOnlyCollection<IProtocolLifecycleObserver> _lifecycleObservers;
    private readonly IProtocolEventPublisher? _eventPublisher;
    private readonly ILoggingService? _loggingService;
    private readonly ILogger<ServiceManager<TService, TPage>>? _logger;
    private readonly ConcurrentDictionary<IServiceRuntime, ServiceRuntimeContext> _runtimeContexts = new();
    private bool _disposed;

    public ServiceManager(
        IServiceCatalog serviceCatalog,
        IServiceRuntimeFactory runtimeFactory,
        IServiceUiRegistry<TService, TPage> uiRegistry,
        IEnumerable<IProtocolLifecycleObserver>? lifecycleObservers = null,
        IProtocolEventPublisher? eventPublisher = null,
        ILoggingService? loggingService = null,
        ILogger<ServiceManager<TService, TPage>>? logger = null)
    {
        _serviceCatalog = serviceCatalog ?? throw new ArgumentNullException(nameof(serviceCatalog));
        _runtimeFactory = runtimeFactory ?? throw new ArgumentNullException(nameof(runtimeFactory));
        _uiRegistry = uiRegistry ?? throw new ArgumentNullException(nameof(uiRegistry));
        _eventPublisher = eventPublisher;
        _loggingService = loggingService;
        _logger = logger;
        _lifecycleObservers = (lifecycleObservers ?? Array.Empty<IProtocolLifecycleObserver>()).ToArray();
        _serviceCatalog.DescriptorsChanged += OnDescriptorsChanged;
    }

    /// <summary>Gets the registry responsible for UI integration.</summary>
    public IServiceUiRegistry<TService, TPage> UiRegistry => _uiRegistry;

    /// <summary>Raised when the underlying service catalog reports descriptor changes.</summary>
    public event EventHandler? DescriptorsChanged;

    /// <summary>
    /// Creates a runtime context for the specified descriptor identifier.
    /// </summary>
    public ServiceRuntimeContext CreateRuntimeContext(
        string descriptorId,
        string displayName,
        IReadOnlyCollection<string> associatedServices,
        object? payload = null)
    {
        if (descriptorId is null)
        {
            throw new ArgumentNullException(nameof(descriptorId));
        }

        if (!_serviceCatalog.TryGetById(descriptorId, out var descriptor))
        {
            throw new InvalidOperationException($"Descriptor '{descriptorId}' is not registered.");
        }

        return new ServiceRuntimeContext(descriptor, descriptorId, displayName, associatedServices, payload);
    }

    /// <summary>
    /// Starts a runtime for the supplied context, coordinating lifecycle notifications.
    /// </summary>
    public async Task<IServiceRuntime> StartRuntimeAsync(ServiceRuntimeContext context, CancellationToken cancellationToken = default)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        Log($"Starting runtime for descriptor '{context.DescriptorId}'.", LogLevel.Information);

        var runtime = _runtimeFactory.Create(context);
        var startupSucceeded = false;

        try
        {
            if (!_runtimeContexts.TryAdd(runtime, context))
            {
                _runtimeContexts[runtime] = context;
            }

            if (runtime is IProtocolService protocolService)
            {
                await NotifyLifecycleAsync(protocolService, o => o.OnStartingAsync(protocolService, cancellationToken), "starting").ConfigureAwait(false);
                await PublishLifecycleEventAsync(context, protocolService, ProtocolLifecycleStage.Starting, cancellationToken).ConfigureAwait(false);
            }

            await runtime.StartAsync(cancellationToken).ConfigureAwait(false);

            if (runtime is IProtocolService startedProtocol)
            {
                await NotifyLifecycleAsync(startedProtocol, o => o.OnStartedAsync(startedProtocol, cancellationToken), "started").ConfigureAwait(false);
                await PublishLifecycleEventAsync(context, startedProtocol, ProtocolLifecycleStage.Started, cancellationToken).ConfigureAwait(false);
            }

            startupSucceeded = true;
            return runtime;
        }
        catch
        {
            _runtimeContexts.TryRemove(runtime, out _);
            await TryStopAndDisposeRuntimeAsync(runtime, context, cancellationToken).ConfigureAwait(false);
            throw;
        }
        finally
        {
            if (startupSucceeded)
            {
                Log($"Runtime for descriptor '{context.DescriptorId}' started.", LogLevel.Debug);
            }
        }
    }

    /// <summary>
    /// Stops and disposes the supplied runtime.
    /// </summary>
    public async Task StopRuntimeAsync(IServiceRuntime runtime, CancellationToken cancellationToken = default)
    {
        if (runtime is null)
        {
            throw new ArgumentNullException(nameof(runtime));
        }

        _runtimeContexts.TryRemove(runtime, out var context);

        if (runtime is IProtocolService protocolService && context is not null)
        {
            await NotifyLifecycleAsync(protocolService, o => o.OnStoppingAsync(protocolService, cancellationToken), "stopping").ConfigureAwait(false);
            await PublishLifecycleEventAsync(context, protocolService, ProtocolLifecycleStage.Stopping, cancellationToken).ConfigureAwait(false);
        }

        await runtime.StopAsync(cancellationToken).ConfigureAwait(false);

        if (runtime is IProtocolService stoppedProtocol && context is not null)
        {
            await NotifyLifecycleAsync(stoppedProtocol, o => o.OnStoppedAsync(stoppedProtocol, cancellationToken), "stopped").ConfigureAwait(false);
            await PublishLifecycleEventAsync(context, stoppedProtocol, ProtocolLifecycleStage.Stopped, cancellationToken).ConfigureAwait(false);
        }

        await runtime.DisposeAsync().ConfigureAwait(false);

        var descriptorLabel = context?.DescriptorId ?? "unknown";
        Log($"Runtime for descriptor '{descriptorLabel}' stopped.", LogLevel.Debug);
    }

    private async Task TryStopAndDisposeRuntimeAsync(IServiceRuntime runtime, ServiceRuntimeContext context, CancellationToken cancellationToken)
    {
        try
        {
            await runtime.StopAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log($"Failed to stop runtime for descriptor '{context.DescriptorId}' after startup failure: {ex.Message}", LogLevel.Warning);
        }

        try
        {
            await runtime.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log($"Failed to dispose runtime for descriptor '{context.DescriptorId}' after startup failure: {ex.Message}", LogLevel.Warning);
        }
    }

    private async Task NotifyLifecycleAsync(
        IProtocolService protocolService,
        Func<IProtocolLifecycleObserver, Task> invokeAsync,
        string stage)
    {
        foreach (var observer in _lifecycleObservers)
        {
            try
            {
                await invokeAsync(observer).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log($"Lifecycle observer failure during {stage} for '{protocolService.Name}': {ex.Message}", LogLevel.Warning);
            }
        }
    }

    private Task PublishLifecycleEventAsync(
        ServiceRuntimeContext context,
        IProtocolService protocolService,
        ProtocolLifecycleStage stage,
        CancellationToken cancellationToken)
    {
        if (_eventPublisher is null)
        {
            return Task.CompletedTask;
        }

        var associated = context.AssociatedServices ?? Array.Empty<string>();
        var lifecycleEvent = new ProtocolLifecycleEvent(protocolService.Name, context.DescriptorId, context.DisplayName, stage, associated);
        var publishTask = _eventPublisher.PublishAsync(lifecycleEvent, cancellationToken);

        if (publishTask.IsCompletedSuccessfully)
        {
            return Task.CompletedTask;
        }

        return publishTask.ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                {
                    Log($"Failed to publish lifecycle event for '{context.DescriptorId}': {task.Exception?.GetBaseException().Message}", LogLevel.Warning);
                }
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private void OnDescriptorsChanged(object? sender, EventArgs e)
    {
        DescriptorsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Log(string message, LogLevel level)
    {
        _loggingService?.Log(message, level);
        _logger?.Log(MapLevel(level), message);
    }

    private static Microsoft.Extensions.Logging.LogLevel MapLevel(LogLevel level) => level switch
    {
        LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
        LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
        LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
        LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
        LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
        _ => Microsoft.Extensions.Logging.LogLevel.Information
    };

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _serviceCatalog.DescriptorsChanged -= OnDescriptorsChanged;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Resolves descriptor-backed runtime factories and runtime instances.
/// </summary>
public sealed class DescriptorRuntimeFactory : IServiceRuntimeFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DescriptorRuntimeFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IServiceRuntime Create(ServiceRuntimeContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var descriptor = context.Descriptor ?? throw new InvalidOperationException("Descriptor must be provided in the runtime context.");

        foreach (var binding in descriptor.Factories ?? Array.Empty<ServiceFactoryBinding>())
        {
            if (binding.Kind != ServiceFactoryKind.Runtime)
            {
                continue;
            }

            var resolved = binding.Resolver(_serviceProvider);

            if (resolved is IServiceRuntimeFactory factory && binding.ContractType.IsInstanceOfType(factory))
            {
                return factory.Create(context);
            }

            if (resolved is IServiceRuntime runtime && binding.ContractType.IsInstanceOfType(runtime))
            {
                return runtime;
            }
        }

        throw new InvalidOperationException($"Descriptor '{descriptor.Id}' does not expose a runtime binding.");
    }
}
