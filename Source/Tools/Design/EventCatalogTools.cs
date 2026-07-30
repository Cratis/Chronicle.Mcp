// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.Projections;
using Cratis.Chronicle.Contracts.ReadModels;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Design-time tools for producing a living data dictionary from the event store — every event type,
/// its fields, and which projections, reducers, and reactors consume it. Event names are the ubiquitous
/// language of an event-sourced system; this keeps that language documented and current from the store.
/// </summary>
[McpServerToolType]
public static class EventCatalogTools
{
    /// <summary>
    /// Generates a catalog of every registered event type, its fields, and its consumers.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace to resolve observers in. Defaults to the configured namespace.</param>
    /// <param name="eventType">An optional single event type id to scope the catalog to.</param>
    /// <returns>A catalog entry per event type.</returns>
    [McpServerTool(Name = "generate_event_catalog")]
    [Description("Produces a living data dictionary from the event store: every event type with its fields (from the registered schema) and every projection, reducer, and reactor that consumes it. Ideal onboarding material and grounding for modeling questions. Read-only introspection. Optionally scope to a single event type.")]
    public static async Task<IEnumerable<EventCatalogEntry>> GenerateEventCatalog(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace to resolve observers in. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("An optional single event type id to scope the catalog to.")] string? eventType = null)
    {
        var resolvedEventStore = configuration.ResolveEventStore(eventStore);
        var resolvedNamespace = configuration.ResolveNamespace(@namespace);

        var registrations = (await services.EventTypes.GetAllRegistrations(new GetAllEventTypesRequest { EventStore = resolvedEventStore })).ToList();
        var projections = (await services.Projections.GetAllDefinitions(new GetAllDefinitionsRequest { EventStore = resolvedEventStore })).ToList();
        var observers = (await services.Observers.GetObservers(new AllObserversRequest { EventStore = resolvedEventStore, Namespace = resolvedNamespace })).ToList();
        var readModels = (await services.ReadModels.GetDefinitions(new GetDefinitionsRequest { EventStore = resolvedEventStore })).ReadModels;

        var consumersByEventType = DesignIntrospection.BuildConsumerIndex(projections, observers, readModels);

        return registrations
            .GroupBy(registration => registration.Type.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(registration => registration.Type.Generation).First())
            .Where(registration => string.IsNullOrWhiteSpace(eventType) || string.Equals(registration.Type.Id, eventType, StringComparison.OrdinalIgnoreCase))
            .OrderBy(registration => registration.Type.Id, StringComparer.OrdinalIgnoreCase)
            .Select(registration => new EventCatalogEntry(
                registration.Type.Id,
                registration.Type.Generation,
                registration.Type.Tombstone,
                registration.Owner.ToString(),
                registration.Source.ToString(),
                string.IsNullOrEmpty(registration.EventStore) ? null : registration.EventStore,
                EventSchemaParser.Parse(registration.Schema),
                Consumers(consumersByEventType, registration.Type.Id)))
            .ToList();
    }

    static IReadOnlyList<EventCatalogConsumer> Consumers(IReadOnlyDictionary<string, IReadOnlyList<EventCatalogConsumer>> index, string eventTypeId) =>
        index.TryGetValue(eventTypeId, out var consumers) ? consumers : [];
}
