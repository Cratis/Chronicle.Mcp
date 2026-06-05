// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Tools for working with event types in Chronicle.
/// </summary>
[McpServerToolType]
public static class EventTypeTools
{
    /// <summary>
    /// Lists all registered event types in an event store.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store to get event types from. Defaults to the configured event store.</param>
    /// <returns>A concise descriptor for each registered event type.</returns>
    [McpServerTool(Name = "list_event_types")]
    [Description("Lists all registered event types in an event store, including their id, generation, owner and source. Use to explore the domain schema.")]
    public static async Task<IEnumerable<EventTypeDescriptor>> ListEventTypes(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store to get event types from. Defaults to the configured event store.")] string? eventStore = null)
    {
        var registrations = await services.EventTypes.GetAllRegistrations(new GetAllEventTypesRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore)
        });

        return registrations.Select(registration => new EventTypeDescriptor(
            registration.Type.Id,
            registration.Type.Generation,
            registration.Type.Tombstone,
            registration.Owner.ToString(),
            registration.Source.ToString()));
    }
}
