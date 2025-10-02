namespace DesktopApplicationTemplate.Core.Services.Protocols.Tcp;

/// <summary>
/// Describes a request to execute a TCP script against a test message.
/// </summary>
public sealed class TcpRuntimeExecutionRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TcpRuntimeExecutionRequest"/> class.
    /// </summary>
    public TcpRuntimeExecutionRequest(TcpRuntimeContext context, string script, string testMessage)
    {
        Context = context;
        Script = script;
        TestMessage = testMessage;
    }

    /// <summary>
    /// Gets the runtime context.
    /// </summary>
    public TcpRuntimeContext Context { get; }

    /// <summary>
    /// Gets the script to execute.
    /// </summary>
    public string Script { get; }

    /// <summary>
    /// Gets the test message provided by the user.
    /// </summary>
    public string TestMessage { get; }
}
