// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A single property discovered on an event type's JSON schema, grounded in what the store actually
/// has registered rather than guessed.
/// </summary>
/// <param name="Name">The property name as it appears on the event.</param>
/// <param name="JsonType">The JSON schema type (e.g. string, integer, number, boolean, array, object).</param>
/// <param name="ClrType">The suggested C# type for the property, derived from the JSON type and format.</param>
/// <param name="Format">The JSON schema format when present (e.g. date-time, guid, int32), otherwise null.</param>
/// <param name="Required">Whether the property is listed as required on the schema.</param>
/// <param name="Description">The property description from the schema when present, otherwise null.</param>
public record EventSchemaProperty(
    string Name,
    string JsonType,
    string ClrType,
    string? Format,
    bool Required,
    string? Description);
