using System;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Tcp;

/// <summary>
/// Describes a request to execute a TCP script against a message payload.
/// </summary>
public sealed class TcpRuntimeExecutionRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TcpRuntimeExecutionRequest"/> class.
    /// </summary>
    public TcpRuntimeExecutionRequest(TcpRuntimeContext context, string script, string testMessage, bool isLiveInput = false)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Script = script ?? string.Empty;
        TestMessage = testMessage ?? string.Empty;
        IsLiveInput = isLiveInput;
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
    /// Gets the message supplied for script execution.
    /// </summary>
    public string TestMessage { get; }

    /// <summary>
    /// Gets a value indicating whether the message originated from a live connection.
    /// </summary>
    public bool IsLiveInput { get; }
}
