// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Text.Json;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Sequences;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Tools for reading events from event sequences in Chronicle.
/// </summary>
[McpServerToolType]
public static class EventTools
{
    const string DefaultEventSequenceId = "event-log";

    /// <summary>
    /// Gets events from an event sequence with optional filtering.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="eventSequenceId">The event sequence to read from. Defaults to event-log.</param>
    /// <param name="from">The starting event sequence number.</param>
    /// <param name="to">The optional ending event sequence number.</param>
    /// <param name="eventSourceId">An optional event source id to filter by.</param>
    /// <param name="eventType">An optional comma-separated event type filter (e.g. UserRegistered or UserRegistered+1).</param>
    /// <returns>The matching events.</returns>
    [McpServerTool(Name = "get_events")]
    [Description("Retrieves events from an event sequence with optional filtering by sequence range, event source id, and event type. Returns event headers and JSON content.")]
    public static async Task<IEnumerable<EventDescriptor>> GetEvents(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("The event sequence to read from. Defaults to event-log.")] string eventSequenceId = DefaultEventSequenceId,
        [Description("The starting event sequence number. Defaults to 0.")] ulong from = 0,
        [Description("The optional ending event sequence number.")] ulong? to = null,
        [Description("An optional event source id to filter by.")] string? eventSourceId = null,
        [Description("An optional comma-separated event type filter (e.g. UserRegistered or UserRegistered+1).")] string? eventType = null)
    {
        var request = new FromSequenceNumberRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace),
            EventSequenceId = eventSequenceId,
            FromEventSequenceNumber = from,
            EventSourceId = eventSourceId,
            EventTypeIds = ToEventTypeIds(eventType)
        };

        var response = QueryResults.Unwrap(await services.Sequences.FromSequenceNumber(request));

        return response
            .Where(evt => !to.HasValue || evt.Context.SequenceNumber <= to.Value)
            .Select(evt =>
            {
                var context = evt.Context;
                return new EventDescriptor(
                    context.SequenceNumber,
                    context.EventType?.Id ?? string.Empty,
                    context.EventType?.Generation ?? 0,
                    context.EventSourceId,
                    (DateTimeOffset?)context.Occurred,
                    context.CorrelationId,
                    TryParse(evt.Content));
            });
    }

    /// <summary>
    /// Gets the highest used sequence number (tail) in an event sequence.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="eventSequenceId">The event sequence to inspect. Defaults to event-log.</param>
    /// <param name="eventType">An optional comma-separated event type filter.</param>
    /// <param name="eventSourceId">An optional event source id to filter by.</param>
    /// <returns>The tail (highest used) sequence number.</returns>
    [McpServerTool(Name = "get_tail_sequence_number")]
    [Description("Returns the highest used sequence number (tail) in an event sequence. This is not a total count of events — gaps may exist. Use to gauge how far a sequence has progressed.")]
    public static async Task<ulong> GetTailSequenceNumber(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("The event sequence to inspect. Defaults to event-log.")] string eventSequenceId = DefaultEventSequenceId,
        [Description("An optional comma-separated event type filter.")] string? eventType = null,
        [Description("An optional event source id to filter by.")] string? eventSourceId = null)
    {
        var request = new TailSequenceNumberRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace),
            EventSequenceId = eventSequenceId,
            EventSourceId = eventSourceId,
            EventTypeIds = ToEventTypeIds(eventType)
        };

        var response = QueryResults.Unwrap(await services.Sequences.TailSequenceNumber(request));
        return response.SequenceNumber;
    }

    static string? ToEventTypeIds(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        // The contracts narrow on comma-separated event type identifiers; a
        // generation suffix on an entry ('Type+2') is not part of the identifier.
        return string.Join(',', input
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(entry => entry.Split('+')[0]));
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
