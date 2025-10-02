using System.Threading;
using System.Threading.Tasks;
namespace DesktopApplicationTemplate.Core.Services.Protocols.Tcp;

/// <summary>
/// Executes TCP runtime operations such as script evaluation without UI dependencies.
/// </summary>
public interface ITcpRuntime : IProtocolService
{
    /// <summary>
    /// Initializes the runtime for the specified service context.
    /// </summary>
    /// <param name="context">The runtime context.</param>
    /// <param name="cancellationToken">A token used to cancel the initialization.</param>
    Task<TcpRuntimeState> InitializeAsync(TcpRuntimeContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the supplied script with the provided message.
    /// </summary>
    /// <param name="request">The execution request.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task<TcpRuntimeState> ExecuteAsync(TcpRuntimeExecutionRequest request, CancellationToken cancellationToken = default);
}
