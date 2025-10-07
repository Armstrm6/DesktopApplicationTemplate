using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Provides a placeholder runtime for descriptors that do not yet expose background workflows.
/// </summary>
public sealed class NoOpServiceRuntime : IServiceRuntime
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

/// <summary>
/// Creates <see cref="NoOpServiceRuntime"/> instances for descriptors lacking runtime support.
/// </summary>
public sealed class NoOpServiceRuntimeFactory : IServiceRuntimeFactory
{
    public IServiceRuntime Create(ServiceRuntimeContext context) => new NoOpServiceRuntime();
}
