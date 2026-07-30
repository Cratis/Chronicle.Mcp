// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A deduced structural model of the system living in an event store — entities and their
/// lifecycles read from event type names and schemas, the read surfaces built from them, and the
/// automations reacting to them. The raw material for describing what the system is and telling
/// its story.
/// </summary>
/// <param name="EventStore">The event store the description was deduced from.</param>
/// <param name="Namespace">The namespace observers were resolved in.</param>
/// <param name="Namespaces">All namespaces present in the event store.</param>
/// <param name="Entities">The entities deduced from event type names, each with its lifecycle events.</param>
/// <param name="ReadSurfaces">The read models built from the events — what the business watches.</param>
/// <param name="Automations">The reactors and external observers — what the system does on its own.</param>
/// <param name="UnconsumedEventTypes">Event types no projection, reducer, or reactor consumes — recorded facts nothing acts on yet.</param>
/// <param name="Statistics">Headline numbers for the system.</param>
/// <param name="NarrativeGuidance">Instructions for turning this structural model into a description and a story.</param>
public record SystemDescription(
    string EventStore,
    string Namespace,
    IReadOnlyList<string> Namespaces,
    IReadOnlyList<SystemEntity> Entities,
    IReadOnlyList<SystemReadSurface> ReadSurfaces,
    IReadOnlyList<SystemAutomation> Automations,
    IReadOnlyList<string> UnconsumedEventTypes,
    SystemStatistics Statistics,
    string NarrativeGuidance);
