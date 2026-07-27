// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.Projections;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Design-time tools for auditing event types against what consumes them, surfacing silent data debt:
/// events written that nothing reads, and consumers pointing at event types that no longer exist.
/// </summary>
[McpServerToolType]
public static class EventTypeAuditTools
{
    /// <summary>
    /// Audits registered event types against every projection, reducer, and reactor that consumes them.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace to resolve observers in. Defaults to the configured namespace.</param>
    /// <returns>The audit result listing unconsumed event types and dangling references.</returns>
    [McpServerTool(Name = "audit_unconsumed_event_types")]
    [Description("Cross-references every registered event type against all projections, reducers, and reactors, and reports the event types that nothing consumes (data being written that no read model or reactor reads) plus the inverse: consumers that reference an event type id which no longer exists. Use as a periodic data-debt health check. Read-only introspection — grounded entirely in the store's registry and observer definitions.")]
    public static async Task<EventTypeAuditResult> AuditUnconsumedEventTypes(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace to resolve observers in. Defaults to the configured namespace.")] string? @namespace = null)
    {
        var resolvedEventStore = configuration.ResolveEventStore(eventStore);
        var resolvedNamespace = configuration.ResolveNamespace(@namespace);

        var registrations = (await services.EventTypes.GetAllRegistrations(new GetAllEventTypesRequest { EventStore = resolvedEventStore })).ToList();
        var projections = (await services.Projections.GetAllDefinitions(new GetAllDefinitionsRequest { EventStore = resolvedEventStore })).ToList();
        var observers = (await services.Observers.GetObservers(new AllObserversRequest { EventStore = resolvedEventStore, Namespace = resolvedNamespace })).ToList();

        var references = CollectReferences(projections, observers);
        var consumedIds = references.Select(reference => reference.EventTypeId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var registeredIds = registrations
            .GroupBy(registration => registration.Type.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(registration => registration.Type.Generation).First())
            .ToList();

        var unconsumed = registeredIds
            .Where(registration => !consumedIds.Contains(registration.Type.Id))
            .OrderBy(registration => registration.Type.Id, StringComparer.OrdinalIgnoreCase)
            .Select(registration => new UnconsumedEventType(
                registration.Type.Id,
                registration.Type.Generation,
                registration.Owner.ToString(),
                registration.Type.Tombstone))
            .ToList();

        var registeredIdSet = registeredIds.Select(registration => registration.Type.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var dangling = references
            .Where(reference => !registeredIdSet.Contains(reference.EventTypeId))
            .Distinct()
            .OrderBy(reference => reference.EventTypeId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new EventTypeAuditResult(resolvedEventStore, resolvedNamespace, registeredIdSet.Count, unconsumed, dangling);
    }

    static List<DanglingEventTypeReference> CollectReferences(IEnumerable<ProjectionDefinition> projections, IEnumerable<ObserverInformation> observers)
    {
        var references = new List<DanglingEventTypeReference>();

        foreach (var projection in projections)
        {
            references.AddRange(DesignIntrospection.CollectConsumedEventTypeIds(projection)
                .Select(id => new DanglingEventTypeReference(id, nameof(ObserverType.Projection), projection.Identifier)));
        }

        foreach (var observer in observers)
        {
            references.AddRange((observer.EventTypes ?? [])
                .Where(eventType => !string.IsNullOrEmpty(eventType.Id))
                .Select(eventType => new DanglingEventTypeReference(eventType.Id, observer.Type.ToString(), observer.Id)));
        }

        return references;
    }
}
