// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Tools for discovering event stores and namespaces on the Chronicle server.
/// </summary>
[McpServerToolType]
public static class EventStoreTools
{
    /// <summary>
    /// Lists all event stores registered on the Chronicle server.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <returns>The names of all event stores.</returns>
    [McpServerTool(Name = "list_event_stores")]
    [Description("Lists all event stores registered on the Chronicle server. Use to discover valid event store names.")]
    public static async Task<IEnumerable<string>> ListEventStores(IServices services) =>
        await services.EventStores.GetEventStores();

    /// <summary>
    /// Lists all namespaces within an event store.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store to list namespaces for. Defaults to the configured event store.</param>
    /// <returns>The names of all namespaces in the event store.</returns>
    [McpServerTool(Name = "list_namespaces")]
    [Description("Lists all namespaces within an event store. Use to discover valid namespace names.")]
    public static async Task<IEnumerable<string>> ListNamespaces(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store to list namespaces for. Defaults to the configured event store.")] string? eventStore = null) =>
        await services.Namespaces.GetNamespaces(new GetNamespacesRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore)
        });
}
