using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Csv;

internal sealed class CsvColumnDefinitionConverter : JsonConverter<CsvColumnDefinition>
{
    public override CsvColumnDefinition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new CsvColumnDefinition();
        }

        using var document = JsonDocument.ParseValue(ref reader);
        var element = document.RootElement;
        var column = new CsvColumnDefinition();

        if (element.TryGetProperty(nameof(CsvColumnDefinition.Name), out var nameProp) &&
            nameProp.ValueKind == JsonValueKind.String)
        {
            column.Name = nameProp.GetString() ?? column.Name;
        }

        if (element.TryGetProperty(nameof(CsvColumnDefinition.Expression), out var expressionProp) &&
            expressionProp.ValueKind == JsonValueKind.String)
        {
            column.Expression = expressionProp.GetString() ?? string.Empty;
        }

        if (element.TryGetProperty(nameof(CsvColumnDefinition.Format), out var formatProp) &&
            formatProp.ValueKind == JsonValueKind.String)
        {
            column.Format = formatProp.GetString();
        }

        if (string.IsNullOrWhiteSpace(column.Expression))
        {
            if (element.TryGetProperty("Script", out var legacyScriptProp) &&
                legacyScriptProp.ValueKind == JsonValueKind.String)
            {
                column.Expression = legacyScriptProp.GetString() ?? string.Empty;
            }
            else if (element.TryGetProperty("Service", out var legacyServiceProp) &&
                     legacyServiceProp.ValueKind == JsonValueKind.String)
            {
                var serviceName = legacyServiceProp.GetString();
                column.Expression = BuildLegacyExpression(serviceName, column.Name);
            }
        }

        return column;
    }

    public override void Write(Utf8JsonWriter writer, CsvColumnDefinition value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(CsvColumnDefinition.Name), value.Name);
        writer.WriteString(nameof(CsvColumnDefinition.Expression), value.Expression ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(value.Format))
        {
            writer.WriteString(nameof(CsvColumnDefinition.Format), value.Format);
        }
        writer.WriteEndObject();
    }

    private static string BuildLegacyExpression(string? serviceName, string columnName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            return string.Empty;
        }

        var trimmedService = serviceName.Trim();
        if (columnName.EndsWith(" Sent", StringComparison.OrdinalIgnoreCase))
        {
            return $"{{{trimmedService}.OutputMessage}}";
        }

        return $"{{{trimmedService}.InputMessage}}";
    }
}
