// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Parses the JSON schema carried on a Chronicle event type registration into a flat list of
/// properties, so design-time tools can ground their suggestions in the event's real shape.
/// </summary>
public static class EventSchemaParser
{
    const string DefaultType = "object";

    static readonly Dictionary<string, string> _formatClrTypes = new(StringComparer.Ordinal)
    {
        ["guid"] = "Guid",
        ["uuid"] = "Guid",
        ["date-time"] = "DateTimeOffset",
        ["date"] = "DateOnly",
        ["time"] = "TimeOnly",
        ["int32"] = "int",
        ["int64"] = "long"
    };

    static readonly Dictionary<string, string> _jsonTypeClrTypes = new(StringComparer.Ordinal)
    {
        ["string"] = "string",
        ["integer"] = "int",
        ["number"] = "double",
        ["boolean"] = "bool"
    };

    /// <summary>
    /// Parses the top-level properties of an event type's JSON schema.
    /// </summary>
    /// <param name="schema">The JSON schema string from an event type registration.</param>
    /// <returns>The properties declared on the schema, or an empty collection when the schema is missing or unparseable.</returns>
    public static IReadOnlyList<EventSchemaProperty> Parse(string? schema)
    {
        if (string.IsNullOrWhiteSpace(schema))
        {
            return [];
        }

        JsonElement root;
        try
        {
            using var document = JsonDocument.Parse(schema);
            root = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return [];
        }

        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("properties", out var properties) || properties.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        var required = ReadRequired(root);

        return properties.EnumerateObject()
            .Select(property => ToSchemaProperty(property.Name, property.Value, required.Contains(property.Name)))
            .ToList();
    }

    static HashSet<string> ReadRequired(JsonElement root)
    {
        if (!root.TryGetProperty("required", out var required) || required.ValueKind != JsonValueKind.Array)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        return required.EnumerateArray()
            .Where(entry => entry.ValueKind == JsonValueKind.String)
            .Select(entry => entry.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
    }

    static EventSchemaProperty ToSchemaProperty(string name, JsonElement definition, bool required)
    {
        var jsonType = ReadType(definition);
        var format = ReadString(definition, "format");
        var description = ReadString(definition, "description");
        var clrType = ToClrType(jsonType, format, definition);

        return new EventSchemaProperty(name, jsonType, clrType, format, required, description);
    }

    static string ReadType(JsonElement definition)
    {
        if (!definition.TryGetProperty("type", out var type))
        {
            return DefaultType;
        }

        return type.ValueKind switch
        {
            JsonValueKind.String => type.GetString() ?? DefaultType,
            JsonValueKind.Array => FirstNonNullType(type),
            _ => DefaultType
        };
    }

    static string FirstNonNullType(JsonElement types) =>
        types.EnumerateArray()
            .Where(entry => entry.ValueKind == JsonValueKind.String)
            .Select(entry => entry.GetString())
            .FirstOrDefault(entry => !string.Equals(entry, "null", StringComparison.Ordinal)) ?? DefaultType;

    static string ToClrType(string jsonType, string? format, JsonElement definition)
    {
        if (format is not null && _formatClrTypes.TryGetValue(format, out var formatClrType))
        {
            return formatClrType;
        }

        if (_jsonTypeClrTypes.TryGetValue(jsonType, out var jsonTypeClrType))
        {
            return jsonTypeClrType;
        }

        return string.Equals(jsonType, "array", StringComparison.Ordinal)
            ? $"IEnumerable<{ElementClrType(definition)}>"
            : ReferenceTypeName(definition);
    }

    static string ElementClrType(JsonElement definition)
    {
        if (!definition.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Object)
        {
            return "object";
        }

        var itemType = ReadType(items);
        var itemFormat = ReadString(items, "format");
        return ToClrType(itemType, itemFormat, items);
    }

    static string ReferenceTypeName(JsonElement definition)
    {
        var reference = ReadString(definition, "$ref");
        if (string.IsNullOrWhiteSpace(reference))
        {
            return "object";
        }

        var lastSegment = reference.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        return string.IsNullOrWhiteSpace(lastSegment) ? "object" : lastSegment;
    }

    static string? ReadString(JsonElement definition, string propertyName) =>
        definition.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
