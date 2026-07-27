// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Chronicle.Contracts.Events;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Turns resolved event type schemas into a model-bound read model + projection scaffold. Pure code
/// generation grounded in the fields the events actually carry — no store access happens here.
/// </summary>
public static class ReadModelScaffolder
{
    /// <summary>
    /// Collects the read model fields from the resolved event type registrations, de-duplicating by name.
    /// </summary>
    /// <param name="registrations">The resolved event type registrations to draw fields from.</param>
    /// <param name="notes">A collection that receives notes about type conflicts or skipped fields.</param>
    /// <returns>The chosen fields, in first-seen order.</returns>
    public static IReadOnlyList<ScaffoldedField> CollectFields(IReadOnlyList<EventTypeRegistration> registrations, ICollection<string> notes)
    {
        var fields = new List<ScaffoldedField>();
        var seen = new Dictionary<string, ScaffoldedField>(StringComparer.OrdinalIgnoreCase);

        foreach (var registration in registrations)
        {
            foreach (var property in EventSchemaParser.Parse(registration.Schema))
            {
                var name = PascalCase(property.Name);
                if (string.Equals(name, "Id", StringComparison.Ordinal))
                {
                    continue;
                }

                if (seen.TryGetValue(name, out var existing))
                {
                    if (!string.Equals(existing.ClrType, property.ClrType, StringComparison.Ordinal))
                    {
                        notes.Add($"Property '{name}' appears as {existing.ClrType} (from {existing.SourceEventType}) and {property.ClrType} (from {registration.Type.Id}); kept the first. Review the type.");
                    }

                    continue;
                }

                var field = new ScaffoldedField(name, property.ClrType, registration.Type.Id);
                seen[name] = field;
                fields.Add(field);
            }
        }

        return fields;
    }

    /// <summary>
    /// Generates the C# source for a model-bound read model and its projection.
    /// </summary>
    /// <param name="readModelName">The read model name.</param>
    /// <param name="namespaceName">The C# namespace to place the read model in.</param>
    /// <param name="eventTypeIds">The event type ids to wire up via <c>[FromEvent&lt;T&gt;]</c>.</param>
    /// <param name="fields">The read model fields.</param>
    /// <returns>The generated C# source.</returns>
    public static string Generate(string readModelName, string namespaceName, IReadOnlyList<string> eventTypeIds, IReadOnlyList<ScaffoldedField> fields)
    {
        var parameterList = new List<string> { "    Guid Id" };
        parameterList.AddRange(fields.Select(field => $"    {field.ClrType} {field.Name}"));

        var lines = new List<string>
        {
            "// Copyright (c) Cratis. All rights reserved.",
            "// Licensed under the MIT license. See LICENSE file in the project root for full license information.",
            string.Empty,
            "using MongoDB.Driver;",
            string.Empty,
            $"namespace {namespaceName};",
            string.Empty,
            "/// <summary>",
            $"/// {readModelName} read model, scaffolded from {string.Join(", ", eventTypeIds)}.",
            "/// Review the field selection and replace the Guid identity with a strongly-typed EventSourceId concept before use.",
            "/// </summary>",
            "[ReadModel]"
        };

        lines.AddRange(eventTypeIds.Select(id => $"[FromEvent<{id}>]"));
        lines.Add($"public record {readModelName}(");
        lines.Add(string.Join(",\n", parameterList) + ")");
        lines.Add("{");
        lines.Add("    /// <summary>");
        lines.Add($"    /// Queries all {readModelName} instances.");
        lines.Add("    /// </summary>");
        lines.Add("    /// <param name=\"collection\">The read model collection.</param>");
        lines.Add($"    /// <returns>All {readModelName} instances.</returns>");
        lines.Add($"    public static IQueryable<{readModelName}> All{readModelName}(IMongoCollection<{readModelName}> collection) =>");
        lines.Add("        collection.AsQueryable();");
        lines.Add("}");

        return string.Join('\n', lines) + "\n";
    }

    static string PascalCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return char.ToUpper(value[0], CultureInfo.InvariantCulture) + value[1..];
    }
}
