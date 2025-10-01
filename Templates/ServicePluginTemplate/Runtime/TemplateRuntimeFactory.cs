using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Logging;

namespace ServicePluginTemplate.Runtime;

/// <summary>
/// Provides a sample runtime workflow that logs lifecycle events.
/// </summary>
public sealed class TemplateRuntimeFactory : IServiceRuntimeFactory
{
    private readonly ILogger<TemplateRuntimeFactory> logger;

    public TemplateRuntimeFactory(ILogger<TemplateRuntimeFactory> logger)
    {
        this.logger = logger;
    }

    public IServiceRuntime Create(ServiceRuntimeContext context) => new TemplateServiceRuntime(logger, context);

    private sealed class TemplateServiceRuntime : IServiceRuntime
    {
        private readonly ILogger logger;
        private readonly ServiceRuntimeContext context;

        public TemplateServiceRuntime(ILogger logger, ServiceRuntimeContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        public ValueTask DisposeAsync()
        {
            logger.LogInformation("Disposing runtime for {DescriptorId}.", context.DescriptorId);
            return ValueTask.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting sample runtime for {DescriptorId}.", context.DescriptorId);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping sample runtime for {DescriptorId}.", context.DescriptorId);
            return Task.CompletedTask;
        }
    }
}
