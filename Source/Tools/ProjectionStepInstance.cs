// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A read model instance or event content parsed from the JSON the server returned.
/// </summary>
/// <param name="Value">The parsed JSON, or null when there is none or it is not valid.</param>
/// <param name="Readable">Whether the server's text was blank or valid JSON.</param>
sealed record ProjectionStepInstance(JsonElement? Value, bool Readable)
{
    /// <summary>
    /// Gets an instance with no state, as before the first event.
    /// </summary>
    public static ProjectionStepInstance Empty { get; } = new(null, true);

    /// <summary>
    /// Gets a value indicating whether the instance holds any state.
    /// </summary>
    public bool HasState => Value is { } value && value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => false,
        JsonValueKind.Object => value.EnumerateObject().Any(),
        _ => true
    };

    /// <summary>
    /// Parses the JSON text the server returned.
    /// </summary>
    /// <param name="json">The JSON text.</param>
    /// <returns>The parsed instance; unreadable when the text is not valid JSON.</returns>
    public static ProjectionStepInstance From(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Empty;
        }

        try
        {
            return new(JsonSerializer.Deserialize<JsonElement>(json), true);
        }
        catch (JsonException)
        {
            return new(null, false);
        }
    }
}
