// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.ReadModels;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Design-time tool that deduces what the system in an event store is and is for. It reads the
/// registered event types, their schemas, and the projections, reducers, and reactors around them,
/// then clusters events into entities and lifecycles so an assistant can describe the system in
/// domain language and tell its story — grounded in what the store actually contains.
/// </summary>
[McpServerToolType]
public static class SystemDescriptionTools
{
    const string NarrativeGuidance =
        "This is a structural model deduced from the store's registered metadata, not a finished description — " +
        "turn it into one. (1) Say what the system is and is for: read the entity names, their events, and the " +
        "property vocabulary as the system's ubiquitous language, and state the domain in plain words. " +
        "(2) Tell the system's story entity by entity: how each comes to life (Creation events), what can happen " +
        "to it along the way (Mutation and Activity events and the data they carry), how mistakes are handled " +
        "(Correction events), and how its life ends (Termination events). (3) Weave in the read surfaces as what " +
        "the business watches, and the automations as what the system does on its own when facts occur. " +
        "(4) Mention unconsumed event types as facts the system records but does not yet act on. " +
        "Stay grounded: describe only what the model evidences, and prefer the domain's own words over technical vocabulary.";

    /// <summary>
    /// Describes the system in an event store by deducing entities, lifecycles, read surfaces, and
    /// automations from the registered metadata.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store to describe. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace to resolve observers in. Defaults to the configured namespace.</param>
    /// <returns>The deduced <see cref="SystemDescription"/>.</returns>
    [McpServerTool(Name = "describe_system")]
    [Description("Deduces what the system in an event store is and is for. Clusters event types into entities by name, places every event in its entity's lifecycle (creation, mutation, activity, correction, termination), and maps read surfaces and automations. Use it to describe a system in domain language or to tell the story of how the system behaves — the result carries narrative guidance for doing exactly that. Read-only introspection.")]
    public static async Task<SystemDescription> DescribeSystem(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store to describe. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace to resolve observers in. Defaults to the configured namespace.")] string? @namespace = null)
    {
        var resolvedEventStore = configuration.ResolveEventStore(eventStore);
        var resolvedNamespace = configuration.ResolveNamespace(@namespace);

        var registrations = (await services.EventTypes.GetAllRegistrations(new GetAllEventTypesRequest { EventStore = resolvedEventStore })).ToList();
        var projections = (await services.Projections.GetAllDefinitions(new Contracts.Projections.GetAllDefinitionsRequest { EventStore = resolvedEventStore })).ToList();
        var observers = (await services.Observers.GetObservers(new AllObserversRequest { EventStore = resolvedEventStore, Namespace = resolvedNamespace })).ToList();
        var readModels = (await services.ReadModels.GetDefinitions(new GetDefinitionsRequest { EventStore = resolvedEventStore })).ReadModels;
        var namespaces = (await services.Namespaces.GetNamespaces(new GetNamespacesRequest { EventStore = resolvedEventStore })).ToList();

        var schemasByEventType = LatestGenerations(registrations)
            .ToDictionary(
                registration => registration.Type.Id,
                registration => EventSchemaParser.Parse(registration.Schema),
                StringComparer.OrdinalIgnoreCase);

        var entities = SystemModelAnalyzer.BuildEntities(schemasByEventType);
        var consumerIndex = DesignIntrospection.BuildConsumerIndex(projections, observers, readModels);
        var readSurfaces = ReadSurfaces(consumerIndex);
        var automations = Automations(consumerIndex);
        var unconsumed = schemasByEventType.Keys
            .Where(eventTypeId => !consumerIndex.ContainsKey(eventTypeId))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new SystemDescription(
            resolvedEventStore,
            resolvedNamespace,
            namespaces,
            entities,
            readSurfaces,
            automations,
            unconsumed,
            new SystemStatistics(schemasByEventType.Count, entities.Count, readSurfaces.Count, automations.Count, namespaces.Count),
            NarrativeGuidance);
    }

    static IEnumerable<EventTypeRegistration> LatestGenerations(IEnumerable<EventTypeRegistration> registrations) =>
        registrations
            .GroupBy(registration => registration.Type.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(registration => registration.Type.Generation).First());

    static bool BuildsReadModel(EventCatalogConsumer consumer) =>
        string.Equals(consumer.Type, nameof(ObserverType.Projection), StringComparison.Ordinal) ||
        string.Equals(consumer.Type, nameof(ObserverType.Reducer), StringComparison.Ordinal);

    static List<SystemReadSurface> ReadSurfaces(IReadOnlyDictionary<string, IReadOnlyList<EventCatalogConsumer>> consumerIndex) =>
        InvertIndex(consumerIndex, BuildsReadModel)
            .Select(entry => new SystemReadSurface(entry.Consumer.ReadModel ?? entry.Consumer.Id, entry.Consumer.Type, entry.EventTypes))
            .OrderBy(surface => surface.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    static List<SystemAutomation> Automations(IReadOnlyDictionary<string, IReadOnlyList<EventCatalogConsumer>> consumerIndex) =>
        InvertIndex(consumerIndex, consumer => !BuildsReadModel(consumer))
            .Select(entry => new SystemAutomation(entry.Consumer.Id, entry.Consumer.Type, entry.EventTypes))
            .OrderBy(automation => automation.Identifier, StringComparer.OrdinalIgnoreCase)
            .ToList();

    static IEnumerable<(EventCatalogConsumer Consumer, IReadOnlyList<string> EventTypes)> InvertIndex(
        IReadOnlyDictionary<string, IReadOnlyList<EventCatalogConsumer>> consumerIndex,
        Func<EventCatalogConsumer, bool> predicate) =>
        consumerIndex
            .SelectMany(entry => entry.Value
                .Where(predicate)
                .Select(consumer => (consumer, EventType: entry.Key)))
            .GroupBy(pair => (pair.consumer.Type, pair.consumer.Id))
            .Select(group => (
                group.First().consumer,
                (IReadOnlyList<string>)group
                    .Select(pair => pair.EventType)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToList()));
}
