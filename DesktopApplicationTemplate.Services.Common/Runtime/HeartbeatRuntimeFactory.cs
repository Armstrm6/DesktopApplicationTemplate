using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.Services.Common.Runtime;

internal sealed class HeartbeatRuntimeFactory : IServiceRuntimeFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptionsMonitor<HeartbeatRuntimeOptions> _options;

    public HeartbeatRuntimeFactory(ILoggerFactory loggerFactory, IOptionsMonitor<HeartbeatRuntimeOptions> options)
    {
        _loggerFactory = loggerFactory;
        _options = options;
    }

    public IServiceRuntime Create(ServiceRuntimeContext context)
    {
        var logger = _loggerFactory.CreateLogger<HeartbeatRuntimeService>();
        var options = _options.CurrentValue;
        return new HeartbeatRuntimeService(logger, options, context.DisplayName);
    }
}
