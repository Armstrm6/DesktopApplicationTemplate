using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Csv;

public static class CsvExpressionEvaluator
{
    private static readonly Regex TokenRegex = new(@"\{([A-Za-z0-9_]+)\.([A-Za-z0-9_]+)\}", RegexOptions.Compiled);

    public static IReadOnlyList<(string Service, string Attribute)> GetReferences(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return Array.Empty<(string, string)>();
        }

        var matches = TokenRegex.Matches(expression);
        if (matches.Count == 0)
        {
            return Array.Empty<(string, string)>();
        }

        var results = new List<(string Service, string Attribute)>(matches.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in matches)
        {
            if (match.Groups.Count < 3)
            {
                continue;
            }

            var service = match.Groups[1].Value;
            var attribute = match.Groups[2].Value;
            var key = string.Concat(service, "\u001f", attribute);

            if (seen.Add(key))
            {
                results.Add((service, attribute));
            }
        }

        return results;
    }

    public static string Evaluate(string? expression, IMessageRoutingService routingService, string? referencingServiceName)
    {
        if (routingService is null)
        {
            throw new ArgumentNullException(nameof(routingService));
        }

        if (string.IsNullOrWhiteSpace(expression))
        {
            return string.Empty;
        }

        return routingService.ResolveTokens(expression, referencingServiceName);
    }

    public static string ApplyFormat(string value, string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return value;
        }

        try
        {
            if (format.Contains('{'))
            {
                return string.Format(CultureInfo.InvariantCulture, format, value);
            }

            return string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", value);
        }
        catch (FormatException)
        {
            return value;
        }
    }

    public static bool ReferencesAttribute(string? expression, string serviceName, string attributeName)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        var normalizedService = serviceName ?? string.Empty;
        var normalizedAttribute = attributeName ?? string.Empty;
        foreach (var reference in GetReferences(expression))
        {
            if (string.Equals(reference.Service, normalizedService, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(reference.Attribute, normalizedAttribute, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
