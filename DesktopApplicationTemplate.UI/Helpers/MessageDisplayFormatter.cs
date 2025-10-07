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
        /// Formats a message for display using readable control character tokens while keeping the
        /// original payload intact. Service reference prefixes are intentionally omitted so the
        /// displayed text mirrors the exact message that was transmitted or received.
        /// </summary>
        /// <param name="message">The message text to format.</param>
        /// <param name="isIncoming">Unused. Present for backward compatibility with previous callers.</param>
        /// <param name="serviceType">Unused. Present for backward compatibility with previous callers.</param>
        /// <param name="serviceName">Unused. Present for backward compatibility with previous callers.</param>
        /// <returns>A formatted string suitable for UI presentation.</returns>
        public static string FormatForService(string? message, bool isIncoming, ServiceType serviceType, string serviceName)
        {
            _ = isIncoming;
            _ = serviceType;
            _ = serviceName;
            return FormatControlCharacters(message);
        }
    }
}
