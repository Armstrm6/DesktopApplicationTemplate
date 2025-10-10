using System;

namespace DesktopApplicationTemplate.Core.Services;

internal static class MessageRoutingAttributeHelper
{
    internal const string InputAttributeName = "InputMessage";
    internal const string OutputAttributeName = "OutputMessage";

    public static string Normalize(string attributeName, out MessageRoutingDirection? direction)
    {
        if (attributeName is null)
        {
            throw new ArgumentNullException(nameof(attributeName));
        }

        var trimmed = attributeName.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(attributeName));
        }

        if (string.Equals(trimmed, InputAttributeName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "LastInputMessage", StringComparison.OrdinalIgnoreCase))
        {
            direction = MessageRoutingDirection.Input;
            return InputAttributeName;
        }

        if (string.Equals(trimmed, OutputAttributeName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "LastOutputMessage", StringComparison.OrdinalIgnoreCase))
        {
            direction = MessageRoutingDirection.Output;
            return OutputAttributeName;
        }

        direction = null;
        return trimmed;
    }

    public static string FromDirection(MessageRoutingDirection direction)
    {
        return direction == MessageRoutingDirection.Input
            ? InputAttributeName
            : OutputAttributeName;
    }
}
