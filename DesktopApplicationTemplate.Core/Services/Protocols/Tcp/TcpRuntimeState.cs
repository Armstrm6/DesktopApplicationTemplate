namespace DesktopApplicationTemplate.Core.Services.Protocols.Tcp;

/// <summary>
/// Represents the state produced after executing a TCP runtime operation.
/// </summary>
public sealed class TcpRuntimeState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TcpRuntimeState"/> class.
    /// </summary>
    public TcpRuntimeState(string script, string testMessage, string outputMessage)
    {
        Script = script;
        TestMessage = testMessage;
        OutputMessage = outputMessage;
    }

    /// <summary>
    /// Gets the effective script used during execution.
    /// </summary>
    public string Script { get; }

    /// <summary>
    /// Gets the test message supplied to the script.
    /// </summary>
    public string TestMessage { get; }

    /// <summary>
    /// Gets the resulting output message.
    /// </summary>
    public string OutputMessage { get; }
}
