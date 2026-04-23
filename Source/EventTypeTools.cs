// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Mcp.Api;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp;

/// <summary>
/// Represents a set of tools for working with event types in Chronicle.
/// </summary>
[McpServerToolType]
public static class EventTypeTools
{
    /// <summary>
    /// Gets all the event types for a specific event store.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store to get from.</param>
    /// <returns>A collection of <see cref="EventType"/> for the specified event store.</returns>
    [McpServerTool(ReadOnly = true), Description("Gets all the event types for a specific event store.")]
    public static async Task<IEnumerable<EventType>> GetEventTypes(ChronicleApiClient chronicleApiClient, string eventStore) =>
        await chronicleApiClient.GetEventTypes(eventStore);

    /// <summary>
    /// Gets all event type registrations including their schemas for a specific event store.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store to get registrations from.</param>
    /// <returns>A collection of <see cref="EventTypeRegistration"/> with type, owner, source and JSON schema.</returns>
    [McpServerTool(ReadOnly = true), Description("Gets all event type registrations including their JSON schemas for a specific event store. Shows type details, owner (Client/Server), source (Code/User), and the full event schema.")]
    public static async Task<IEnumerable<EventTypeRegistration>> GetEventTypeRegistrations(ChronicleApiClient chronicleApiClient, string eventStore) =>
        await chronicleApiClient.GetEventTypeRegistrations(eventStore);
}
