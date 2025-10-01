using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Logging;

namespace HttpRelayPlugin.Runtime;

public sealed class HttpRelayRuntimeFactory : IServiceRuntimeFactory
{
    private readonly ILogger<HttpRelayRuntimeFactory> logger;

    public HttpRelayRuntimeFactory(ILogger<HttpRelayRuntimeFactory> logger)
    {
        this.logger = logger;
    }

    public IServiceRuntime Create(ServiceRuntimeContext context) => new HttpRelayRuntime(logger, context);

    private sealed class HttpRelayRuntime : IServiceRuntime
    {
        private readonly ILogger logger;
        private readonly ServiceRuntimeContext context;

        public HttpRelayRuntime(ILogger logger, ServiceRuntimeContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        public ValueTask DisposeAsync()
        {
            logger.LogInformation("Disposing HTTP relay runtime for {DescriptorId}.", context.DescriptorId);
            return ValueTask.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting HTTP relay runtime for {DescriptorId}.", context.DescriptorId);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping HTTP relay runtime for {DescriptorId}.", context.DescriptorId);
            return Task.CompletedTask;
        }
    }
}
