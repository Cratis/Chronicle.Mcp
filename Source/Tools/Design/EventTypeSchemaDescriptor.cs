// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A grounded description of an event type's shape, taken directly from the schema registered in the store.
/// </summary>
/// <param name="Id">The unique identifier of the event type.</param>
/// <param name="Generation">The generation the schema describes.</param>
/// <param name="Tombstone">Whether the event type represents a tombstone.</param>
/// <param name="Owner">The owner of the event type (e.g. Client or Chronicle).</param>
/// <param name="Source">The source of the event type registration (e.g. Code).</param>
/// <param name="ForeignEventStore">The originating event store when the type is received from another store, otherwise null.</param>
/// <param name="Properties">The properties declared on the event type schema.</param>
public record EventTypeSchemaDescriptor(
    string Id,
    uint Generation,
    bool Tombstone,
    string Owner,
    string Source,
    string? ForeignEventStore,
    IReadOnlyList<EventSchemaProperty> Properties);
