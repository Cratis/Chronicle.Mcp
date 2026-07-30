// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Text.Json;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Contracts.EventSequences;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Design-time tool that answers ad-hoc questions by projecting events on demand. Given the event
/// types involved, it reads the matching events from the store and folds them into per-event-source
/// instances the way an AutoMap projection would — without registering or persisting anything.
/// This is what turns "show all employees with all details" into a result set.
/// </summary>
[McpServerToolType]
public static class AdHocProjectionTools
{
    const string DefaultEventSequenceId = "event-log";

    const string Guidance =
        "Present the instances in the domain's language — a table works well when they share a shape. Later events " +
        "overwrite earlier property values by name (AutoMap semantics), so each instance shows current state. To refine: " +
        "add event types that carry the missing details, add removedWith event types so ended instances drop out, or scope " +
        "to a single eventSourceId for one instance's state. If this projection answers a recurring question, propose making " +
        "it permanent with scaffold_read_model.";

    /// <summary>
    /// Projects events into per-event-source instances on demand.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventTypes">Comma-separated event type ids to fold.</param>
    /// <param name="removedWith">Optional comma-separated event type ids that remove an instance.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="eventSequenceId">The event sequence to read from. Defaults to event-log.</param>
    /// <param name="eventSourceId">An optional event source id to scope to a single instance.</param>
    /// <param name="limit">The maximum number of instances to return. Defaults to 100.</param>
    /// <returns>The <see cref="AdHocProjectionResult"/> with the materialized instances.</returns>
    [McpServerTool(Name = "run_ad_hoc_projection")]
    [Description("Answers ad-hoc queries by projecting events on demand: reads the given event types from the store and folds them into per-event-source instances with AutoMap semantics (later events overwrite earlier property values; removedWith event types drop the instance). Nothing is registered or written. First resolve which event types are involved (describe_system or generate_event_catalog), then call this with them. Use it for natural-language questions like 'show all employees with all details'.")]
    public static async Task<AdHocProjectionResult> RunAdHocProjection(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("Comma-separated event type ids to fold (e.g. EmployeeHired,EmployeeMoved,EmployeePromoted).")] string eventTypes,
        [Description("Optional comma-separated event type ids that remove an instance (e.g. EmployeeTerminated).")] string? removedWith = null,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("The event sequence to read from. Defaults to event-log.")] string eventSequenceId = DefaultEventSequenceId,
        [Description("An optional event source id to scope to a single instance.")] string? eventSourceId = null,
        [Description("The maximum number of instances to return. Defaults to 100.")] int limit = 100)
    {
        var resolvedEventStore = configuration.ResolveEventStore(eventStore);
        var resolvedNamespace = configuration.ResolveNamespace(@namespace);
        var foldTypes = Split(eventTypes);
        var removalTypes = Split(removedWith);

        var request = new GetFromEventSequenceNumberRequest
        {
            EventStore = resolvedEventStore,
            Namespace = resolvedNamespace,
            EventSequenceId = eventSequenceId,
            FromEventSequenceNumber = 0,
            EventSourceId = string.IsNullOrWhiteSpace(eventSourceId) ? null : eventSourceId
        };

        foreach (var eventTypeId in foldTypes.Concat(removalTypes))
        {
            request.EventTypes.Add(new EventType { Id = eventTypeId, Generation = 1u });
        }

        var response = await services.EventSequences.GetEventsFromEventSequenceNumber(request);

        var events = response.Events.Select(evt => new AdHocEvent(
            evt.Context.EventSourceId,
            evt.Context.EventType?.Id ?? string.Empty,
            evt.Context.SequenceNumber,
            (DateTimeOffset?)evt.Context.Occurred,
            TryParse(evt.Content))).ToList();

        var instances = AdHocProjector.Fold(events, removalTypes);

        return new AdHocProjectionResult(
            resolvedEventStore,
            resolvedNamespace,
            foldTypes,
            removalTypes,
            instances.Take(limit).ToList(),
            instances.Count,
            events.Count,
            Guidance);
    }

    static List<string> Split(string? input) =>
        (input ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

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
