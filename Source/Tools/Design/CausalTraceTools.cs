// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Text.Json;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Auditing;
using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Contracts.EventSequences;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Design-time tool for turning an event source's raw event log into a causal narrative — the ordered
/// events plus the correlation and causation metadata that explain why each one happened.
/// </summary>
[McpServerToolType]
public static class CausalTraceTools
{
    const string DefaultEventSequenceId = "event-log";

    /// <summary>
    /// Reads the causal history of an event source in sequence order.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventSourceId">The event source id to trace.</param>
    /// <param name="eventSequenceId">The event sequence to read from. Defaults to event-log.</param>
    /// <param name="eventType">An optional comma-separated event type filter.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <returns>The causal trace for the event source.</returns>
    [McpServerTool(Name = "explain_causal_trace")]
    [Description("Reads the ordered event history for one event source together with each event's correlation id and causation chain, so an agent can narrate what happened and why instead of a support engineer reading raw JSON logs. Read-only. Follow correlation ids to relate an event to the wider flow that produced it.")]
    public static async Task<CausalTrace> ExplainCausalTrace(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event source id to trace (e.g. the order id).")] string eventSourceId,
        [Description("The event sequence to read from. Defaults to event-log.")] string eventSequenceId = DefaultEventSequenceId,
        [Description("An optional comma-separated event type filter.")] string? eventType = null,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null)
    {
        var resolvedEventStore = configuration.ResolveEventStore(eventStore);
        var resolvedNamespace = configuration.ResolveNamespace(@namespace);

        var request = new GetForEventSourceIdAndEventTypesRequest
        {
            EventStore = resolvedEventStore,
            Namespace = resolvedNamespace,
            EventSequenceId = eventSequenceId,
            EventSourceId = eventSourceId
        };

        foreach (var parsed in ParseEventTypes(eventType))
        {
            request.EventTypes.Add(parsed);
        }

        var response = await services.EventSequences.GetForEventSourceIdAndEventTypes(request);

        var events = response.Events
            .OrderBy(evt => evt.Context.SequenceNumber)
            .Select(ToCausalEvent)
            .ToList();

        return new CausalTrace(resolvedEventStore, resolvedNamespace, eventSequenceId, eventSourceId, events.Count, events);
    }

    static CausalEvent ToCausalEvent(AppendedEvent evt)
    {
        var context = evt.Context;
        return new CausalEvent(
            context.SequenceNumber,
            context.EventType?.Id ?? string.Empty,
            context.EventType?.Generation ?? 0,
            context.Occurred,
            context.CorrelationId,
            ToCausedBy(context.CausedBy),
            (context.Causation ?? []).Select(ToCausationEntry).ToList(),
            TryParse(evt.Content));
    }

    static CausedByDescriptor? ToCausedBy(Cratis.Chronicle.Contracts.Identities.Identity? identity) =>
        identity is null
            ? null
            : new CausedByDescriptor(identity.Subject ?? string.Empty, identity.Name ?? string.Empty, identity.UserName ?? string.Empty);

    static CausationEntry ToCausationEntry(Causation causation) =>
        new(
            causation.Type ?? string.Empty,
            causation.Occurred,
            causation.Properties is null ? new Dictionary<string, string>() : new Dictionary<string, string>(causation.Properties));

    static IEnumerable<EventType> ParseEventTypes(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        return input
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(entry =>
            {
                var parts = entry.Split('+');
                return new EventType
                {
                    Id = parts[0],
                    Generation = parts.Length > 1 && uint.TryParse(parts[1], out var generation) ? generation : 1u
                };
            });
    }

    static JsonElement? TryParse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(content);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
