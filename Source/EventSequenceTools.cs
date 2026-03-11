// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Mcp.Api;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp;

/// <summary>
/// Represents a set of tools for querying events from Chronicle event sequences.
/// </summary>
[McpServerToolType]
public static class EventSequenceTools
{
    /// <summary>
    /// Gets events from a specific event sequence in an event store.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store to get events from.</param>
    /// <param name="namespace">The namespace to query. Defaults to 'Default'.</param>
    /// <param name="eventSequenceId">The event sequence identifier to query. Defaults to 'event-log'.</param>
    /// <param name="eventSourceId">Optional event source identifier to filter events by a specific aggregate or entity.</param>
    /// <returns>A collection of <see cref="AppendedEvent"/> with event context and content.</returns>
    [McpServerTool(ReadOnly = true), Description("Gets events from an event sequence. Each event includes its context (type, source, sequence number, timestamp, causation) and its JSON content. Use eventSourceId to filter events for a specific aggregate.")]
    public static async Task<IEnumerable<AppendedEvent>> GetEvents(
        ChronicleApiClient chronicleApiClient,
        string eventStore,
        string? @namespace = "Default",
        string? eventSequenceId = "event-log",
        string? eventSourceId = null) =>
        await chronicleApiClient.GetEvents(eventStore, @namespace ?? "Default", eventSequenceId ?? "event-log", eventSourceId);
}
