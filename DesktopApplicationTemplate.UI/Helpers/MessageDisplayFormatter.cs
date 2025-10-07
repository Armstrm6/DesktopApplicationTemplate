using System;
using System.Collections.Generic;
using System.Text;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Helpers
{
    /// <summary>
    /// Provides helpers for presenting service messages with readable control characters and
    /// standardized service references.
    /// </summary>
    public static class MessageDisplayFormatter
    {
        private static readonly IReadOnlyDictionary<char, string> ControlCharacterTokens = new Dictionary<char, string>
        {
            ['\0'] = "<NUL>",
            ['\u0001'] = "<SOH>",
            ['\u0002'] = "<STX>",
            ['\u0003'] = "<ETX>",
            ['\u0004'] = "<EOT>",
            ['\u0005'] = "<ENQ>",
            ['\u0006'] = "<ACK>",
            ['\a'] = "<BEL>",
            ['\b'] = "<BS>",
            ['\t'] = "<HT>",
            ['\n'] = "<LF>",
            ['\u000B'] = "<VT>",
            ['\f'] = "<FF>",
            ['\r'] = "<CR>",
            ['\u000E'] = "<SO>",
            ['\u000F'] = "<SI>",
            ['\u0010'] = "<DLE>",
            ['\u0011'] = "<DC1>",
            ['\u0012'] = "<DC2>",
            ['\u0013'] = "<DC3>",
            ['\u0014'] = "<DC4>",
            ['\u0015'] = "<NAK>",
            ['\u0016'] = "<SYN>",
            ['\u0017'] = "<ETB>",
            ['\u0018'] = "<CAN>",
            ['\u0019'] = "<EM>",
            ['\u001A'] = "<SUB>",
            ['\u001B'] = "<ESC>",
            ['\u001C'] = "<FS>",
            ['\u001D'] = "<GS>",
            ['\u001E'] = "<RS>",
            ['\u001F'] = "<US>",
            ['\u007F'] = "<DEL>"
        };

        /// <summary>
        /// Converts ASCII control characters to their readable token form.
        /// </summary>
        /// <param name="message">The message that may contain control characters.</param>
        /// <returns>A string where control characters are replaced with their symbolic names.</returns>
        public static string FormatControlCharacters(string? message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(message.Length);
            foreach (var ch in message)
            {
                if (ControlCharacterTokens.TryGetValue(ch, out var token))
                {
                    builder.Append(token);
                }
                else
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Formats a message for display using the standardized service reference conventions.
        /// </summary>
        /// <param name="message">The message text to format.</param>
        /// <param name="isIncoming">Indicates whether the message represents incoming data.</param>
        /// <param name="serviceType">The type of service producing the message.</param>
        /// <param name="serviceName">The name of the service producing the message.</param>
        /// <returns>A formatted string suitable for UI presentation.</returns>
        public static string FormatForService(string? message, bool isIncoming, ServiceType serviceType, string serviceName)
        {
            var formatted = FormatControlCharacters(message);
            if (string.IsNullOrEmpty(formatted))
            {
                return string.Empty;
            }

            if (!ShouldUseServiceReference(serviceType) || string.IsNullOrWhiteSpace(serviceName))
            {
                return formatted;
            }

            var propertyName = isIncoming ? "LastInputMessage" : "LastOutputMessage";
            return string.Create(serviceName.Length + propertyName.Length + formatted.Length + 2, (serviceName, propertyName, formatted),
                (span, state) =>
                {
                    var (svcName, prop, content) = state;
                    var index = 0;
                    svcName.AsSpan().CopyTo(span);
                    index += svcName.Length;
                    span[index++] = '.';
                    prop.AsSpan().CopyTo(span[index..]);
                    index += prop.Length;
                    span[index++] = ':';
                    span[index++] = ' ';
                    content.AsSpan().CopyTo(span[index..]);
                });
        }

        private static bool ShouldUseServiceReference(ServiceType serviceType)
        {
            return serviceType is not ServiceType.Csv
                and not ServiceType.Ftp
                and not ServiceType.FileObserver
                and not ServiceType.Scp;
        }
    }
}
