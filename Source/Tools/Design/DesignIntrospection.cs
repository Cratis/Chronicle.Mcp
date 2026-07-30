// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.Projections;
using Cratis.Chronicle.Contracts.ReadModels;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Shared introspection helpers for the design-time tools. Everything here reads the store's registered
/// schema so suggestions are grounded in what actually exists rather than guessed.
/// </summary>
public static class DesignIntrospection
{
    /// <summary>
    /// Resolves the registration for an event type by name, picking a specific generation or the latest one.
    /// </summary>
    /// <param name="registrations">The registrations returned from the store.</param>
    /// <param name="name">The event type id/name to resolve (case-insensitive).</param>
    /// <param name="generation">The optional specific generation to resolve; the highest generation is used when null.</param>
    /// <returns>The matching <see cref="EventTypeRegistration"/>, or null when no event type matches.</returns>
    public static EventTypeRegistration? ResolveRegistration(IEnumerable<EventTypeRegistration> registrations, string name, uint? generation = null)
    {
        var matches = registrations
            .Where(registration => string.Equals(registration.Type.Id, name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
        {
            return null;
        }

        return generation.HasValue
            ? matches.FirstOrDefault(registration => registration.Type.Generation == generation.Value)
            : matches.OrderByDescending(registration => registration.Type.Generation).First();
    }

    /// <summary>
    /// Collects every event type id consumed by a projection, walking nested and child projections.
    /// </summary>
    /// <param name="projection">The projection definition to inspect.</param>
    /// <returns>The distinct set of event type ids the projection reads from.</returns>
    public static IReadOnlySet<string> CollectConsumedEventTypeIds(ProjectionDefinition projection)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        CollectFromLevel(projection.From, projection.Join, projection.RemovedWith, projection.RemovedWithJoin, projection.FromEventProperty, projection.Children, projection.Nested, ids);
        return ids;
    }

    /// <summary>
    /// Builds an index from event type id to every projection, reducer, and reactor consuming it.
    /// </summary>
    /// <param name="projections">The projection definitions from the store.</param>
    /// <param name="observers">The observers registered in the namespace.</param>
    /// <param name="readModels">The read model definitions, used to resolve observer read model names.</param>
    /// <returns>The consumer index keyed by event type id (case-insensitive).</returns>
    public static IReadOnlyDictionary<string, IReadOnlyList<EventCatalogConsumer>> BuildConsumerIndex(
        IEnumerable<ProjectionDefinition> projections,
        IEnumerable<ObserverInformation> observers,
        IEnumerable<ReadModelDefinition> readModels)
    {
        var readModelByObserverId = readModels
            .Where(readModel => !string.IsNullOrEmpty(readModel.ObserverIdentifier))
            .GroupBy(readModel => readModel.ObserverIdentifier, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => ReadModelName(group.First()), StringComparer.Ordinal);

        var index = new Dictionary<string, List<EventCatalogConsumer>>(StringComparer.OrdinalIgnoreCase);

        foreach (var projection in projections)
        {
            var consumer = new EventCatalogConsumer(nameof(ObserverType.Projection), projection.Identifier, string.IsNullOrWhiteSpace(projection.ReadModel) ? null : projection.ReadModel);
            foreach (var eventTypeId in CollectConsumedEventTypeIds(projection))
            {
                Add(index, eventTypeId, consumer);
            }
        }

        foreach (var observer in observers.Where(observer => observer.Type != ObserverType.Projection))
        {
            var readModel = readModelByObserverId.TryGetValue(observer.Id, out var name) ? name : null;
            var consumer = new EventCatalogConsumer(observer.Type.ToString(), observer.Id, readModel);
            foreach (var eventTypeId in (observer.EventTypes ?? []).Select(eventType => eventType.Id).Where(id => !string.IsNullOrEmpty(id)))
            {
                Add(index, eventTypeId, consumer);
            }
        }

        return index.ToDictionary(entry => entry.Key, entry => (IReadOnlyList<EventCatalogConsumer>)entry.Value, StringComparer.OrdinalIgnoreCase);
    }

    static string? ReadModelName(ReadModelDefinition readModel)
    {
        if (!string.IsNullOrWhiteSpace(readModel.DisplayName))
        {
            return readModel.DisplayName;
        }

        return string.IsNullOrWhiteSpace(readModel.ContainerName) ? null : readModel.ContainerName;
    }

    static void Add(Dictionary<string, List<EventCatalogConsumer>> index, string eventTypeId, EventCatalogConsumer consumer)
    {
        if (!index.TryGetValue(eventTypeId, out var consumers))
        {
            consumers = [];
            index[eventTypeId] = consumers;
        }

        if (!consumers.Exists(existing => existing.Type == consumer.Type && existing.Id == consumer.Id))
        {
            consumers.Add(consumer);
        }
    }

    static void CollectFromLevel(
        IDictionary<EventType, FromDefinition> from,
        IDictionary<EventType, JoinDefinition> join,
        IDictionary<EventType, RemovedWithDefinition> removedWith,
        IDictionary<EventType, RemovedWithJoinDefinition> removedWithJoin,
        FromEventPropertyDefinition? fromEventProperty,
        IDictionary<string, ChildrenDefinition> children,
        IDictionary<string, ChildrenDefinition> nested,
        HashSet<string> ids)
    {
        AddEventTypeIds(from?.Keys, ids);
        AddEventTypeIds(join?.Keys, ids);
        AddEventTypeIds(removedWith?.Keys, ids);
        AddEventTypeIds(removedWithJoin?.Keys, ids);

        if (fromEventProperty?.Event is { } propertyEvent && !string.IsNullOrEmpty(propertyEvent.Id))
        {
            ids.Add(propertyEvent.Id);
        }

        foreach (var child in EnumerateChildren(children).Concat(EnumerateChildren(nested)))
        {
            CollectFromLevel(child.From, child.Join, child.RemovedWith, child.RemovedWithJoin, child.FromEventProperty, child.Children, child.Nested, ids);
        }
    }

    static IEnumerable<ChildrenDefinition> EnumerateChildren(IDictionary<string, ChildrenDefinition>? children) =>
        children?.Values ?? [];

    static void AddEventTypeIds(IEnumerable<EventType>? eventTypes, HashSet<string> ids)
    {
        foreach (var eventType in eventTypes ?? [])
        {
            if (!string.IsNullOrEmpty(eventType.Id))
            {
                ids.Add(eventType.Id);
            }
        }
    }
}
