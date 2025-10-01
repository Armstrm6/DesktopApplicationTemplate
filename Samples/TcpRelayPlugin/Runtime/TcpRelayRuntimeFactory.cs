using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Logging;

namespace TcpRelayPlugin.Runtime;

public sealed class TcpRelayRuntimeFactory : IServiceRuntimeFactory
{
    private readonly ILogger<TcpRelayRuntimeFactory> logger;

    public TcpRelayRuntimeFactory(ILogger<TcpRelayRuntimeFactory> logger)
    {
        this.logger = logger;
    }

    public IServiceRuntime Create(ServiceRuntimeContext context) => new TcpRelayRuntime(logger, context);

    private sealed class TcpRelayRuntime : IServiceRuntime
    {
        private readonly ILogger logger;
        private readonly ServiceRuntimeContext context;

        public TcpRelayRuntime(ILogger logger, ServiceRuntimeContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        public ValueTask DisposeAsync()
        {
            logger.LogInformation("Disposing TCP relay runtime for {DescriptorId}.", context.DescriptorId);
            return ValueTask.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting TCP relay runtime for {DescriptorId}.", context.DescriptorId);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping TCP relay runtime for {DescriptorId}.", context.DescriptorId);
            return Task.CompletedTask;
        }
    }
}
