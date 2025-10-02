namespace DesktopApplicationTemplate.UI.Models;

/// <summary>
/// Represents the outcome of a subscription attempt.
/// </summary>
public class SubscriptionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionResult"/> class.
    /// </summary>
    public SubscriptionResult(string topic, bool isSuccess, string message)
    {
        Topic = topic;
        IsSuccess = isSuccess;
        Message = message;
    }

    /// <summary>
    /// Gets the topic involved in the subscription.
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// Gets a value indicating whether the subscription succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the message to display.
    /// </summary>
    public string Message { get; }
}
