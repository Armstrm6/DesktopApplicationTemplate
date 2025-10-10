using System;
using System.Globalization;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using WpfBrush = System.Windows.Media.Brush;

namespace DesktopApplicationTemplate.UI.ViewModels.Services
{
    internal static class ServiceStateCoordinator
    {
        private static bool _enableCrossServiceLogForwarding;

        internal static event Action? CrossServiceAssociationsClearing;

        internal static Func<string?, IServiceOptionsSerializer?>? OptionsSerializerResolver { get; set; }

        internal static bool EnableCrossServiceLogForwarding
        {
            get => _enableCrossServiceLogForwarding;
            set
            {
                if (_enableCrossServiceLogForwarding == value)
                {
                    return;
                }

                _enableCrossServiceLogForwarding = value;
                if (!value)
                {
                    CrossServiceAssociationsClearing?.Invoke();
                }
            }
        }

        internal static IServiceOptionsSerializer? ResolveOptionsSerializer(string? key)
        {
            return OptionsSerializerResolver?.Invoke(key);
        }

        internal static void TryForwardLog(ServiceListModel source, string message, WpfBrush brush, LogLevel level)
        {
            if (!_enableCrossServiceLogForwarding)
            {
                return;
            }

            var lookup = source.ServiceLookup;
            if (lookup is null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (!TryExtractCrossServiceReference(message, out var typeName, out var serviceName, out var forwardedMessage))
            {
                return;
            }

            if (!ServiceTypeExtensions.TryParse(typeName, out var type))
            {
                return;
            }

            if (!lookup.TryGetService(type, serviceName, out var target) || target is null || ReferenceEquals(target, source))
            {
                return;
            }

            source.EnsureAssociation(target);
            target.AddForwardedLog(forwardedMessage, brush, level);
        }

        private static bool TryExtractCrossServiceReference(string message, out string typeName, out string serviceName, out string forwardedMessage)
        {
            typeName = string.Empty;
            serviceName = string.Empty;
            forwardedMessage = string.Empty;

            ReadOnlySpan<char> span = message.AsSpan().TrimStart();
            span = StripTimestamp(span);
            span = StripLogLevelPrefix(span);
            span = TrimLeadingWhitespace(span);

            if (span.IsEmpty)
            {
                return false;
            }

            var normalized = span.ToString();
            var firstDot = normalized.IndexOf('.');
            if (firstDot <= 0)
            {
                return false;
            }

            var secondDot = normalized.IndexOf('.', firstDot + 1);
            if (secondDot <= firstDot + 1)
            {
                return false;
            }

            typeName = normalized[..firstDot];
            serviceName = normalized[(firstDot + 1)..secondDot];
            forwardedMessage = normalized[(secondDot + 1)..].TrimStart();
            return forwardedMessage.Length > 0;
        }

        private static ReadOnlySpan<char> StripTimestamp(ReadOnlySpan<char> message)
        {
            if (message.Length >= ServiceLogState.TimestampLength)
            {
                var timestampCandidate = message[..ServiceLogState.TimestampLength];
                if (DateTime.TryParseExact(timestampCandidate.ToString(), ServiceLogState.TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    return TrimLeadingWhitespace(message[ServiceLogState.TimestampLength..]);
                }
            }

            return message;
        }

        private static ReadOnlySpan<char> StripLogLevelPrefix(ReadOnlySpan<char> message)
        {
            var span = message;
            while (span.Length > 0 && span[0] == '[')
            {
                var closingIndex = span[1..].IndexOf(']');
                if (closingIndex < 0)
                {
                    break;
                }

                var nextIndex = closingIndex + 2;
                if (nextIndex > span.Length)
                {
                    break;
                }

                span = span[nextIndex..];
                span = TrimLeadingWhitespace(span);
            }

            return span;
        }

        private static ReadOnlySpan<char> TrimLeadingWhitespace(ReadOnlySpan<char> span)
        {
            var index = 0;
            while (index < span.Length && char.IsWhiteSpace(span[index]))
            {
                index++;
            }

            return index == 0 ? span : span[index..];
        }
    }
}
