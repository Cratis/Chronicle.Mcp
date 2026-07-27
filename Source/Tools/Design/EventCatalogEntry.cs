// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A single entry in the domain event catalog — one event type, its fields, and everything that consumes it.
/// </summary>
/// <param name="Id">The event type id — the ubiquitous-language term for this fact.</param>
/// <param name="Generation">The current generation of the event type.</param>
/// <param name="Tombstone">Whether the event type is a tombstone.</param>
/// <param name="Owner">The owner of the event type (e.g. Client or Chronicle).</param>
/// <param name="Source">The source of the registration (e.g. Code).</param>
/// <param name="ForeignEventStore">The originating event store when received from another store, otherwise null.</param>
/// <param name="Properties">The properties declared on the event type schema.</param>
/// <param name="Consumers">Everything that reads this event type (projections, reducers, reactors).</param>
public record EventCatalogEntry(
    string Id,
    uint Generation,
    bool Tombstone,
    string Owner,
    string Source,
    string? ForeignEventStore,
    IReadOnlyList<EventSchemaProperty> Properties,
    IReadOnlyList<EventCatalogConsumer> Consumers);
