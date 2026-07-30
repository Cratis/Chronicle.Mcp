// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Design-time tool that suggests which event types to introduce next. It deduces the system's
/// entities and lifecycles from the registered event types, then finds the gaps: lifecycles
/// without a beginning or an end, and facts set at creation that no event can ever change.
/// </summary>
[McpServerToolType]
public static class SystemEvolutionTools
{
    const string Guidance =
        "These suggestions are deterministic gap findings grounded in the store's registered event types — seeds, " +
        "not finished designs. Refine them with domain knowledge: rename to the domain's own ubiquitous language " +
        "(e.g. a Book is 'Withdrawn', not 'Removed'; an Order is 'Cancelled', not 'Deleted'), discard any that the " +
        "business genuinely never needs (some facts really are immutable), and propose properties drawn from the " +
        "existing schema vocabulary. Keep every event past tense, single purpose, and self-describing, and never " +
        "add a nullable property — an optional fact is its own event.";

    /// <summary>
    /// Suggests event types to introduce next for the system in an event store.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store to analyze. Defaults to the configured event store.</param>
    /// <param name="entity">An optional entity name to scope the suggestions to.</param>
    /// <returns>The <see cref="SystemEvolutionSuggestions"/> with grounded suggestions and refinement guidance.</returns>
    [McpServerTool(Name = "suggest_next_event_types")]
    [Description("Suggests event types to introduce next for the system in an event store. Deduces entities and lifecycles from the registered event type names and schemas, then finds concrete gaps: entities without an explicit creation event, lifecycles with no termination, and creation-time properties no event can ever change. Each suggestion carries the gap it closes and candidate properties from the existing schema. Read-only introspection; refine the results with domain knowledge.")]
    public static async Task<SystemEvolutionSuggestions> SuggestNextEventTypes(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store to analyze. Defaults to the configured event store.")] string? eventStore = null,
        [Description("An optional entity name to scope the suggestions to (case-insensitive).")] string? entity = null)
    {
        var resolvedEventStore = configuration.ResolveEventStore(eventStore);

        var registrations = (await services.EventTypes.GetAllRegistrations(new GetAllEventTypesRequest { EventStore = resolvedEventStore })).ToList();

        var schemasByEventType = registrations
            .GroupBy(registration => registration.Type.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(registration => registration.Type.Generation).First())
            .ToDictionary(
                registration => registration.Type.Id,
                registration => EventSchemaParser.Parse(registration.Schema),
                StringComparer.OrdinalIgnoreCase);

        var entities = SystemModelAnalyzer.BuildEntities(schemasByEventType)
            .Where(candidate => string.IsNullOrWhiteSpace(entity) || string.Equals(candidate.Name, entity, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new SystemEvolutionSuggestions(
            resolvedEventStore,
            entities,
            EventTypeSuggester.Suggest(entities),
            Guidance);
    }
}
