// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Mcp.Api;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp;

/// <summary>
/// Represents a set of tools for discovering and listing event stores in Chronicle.
/// </summary>
[McpServerToolType]
public static class EventStoreTools
{
    /// <summary>
    /// Gets all registered event stores.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <returns>A collection of event store names.</returns>
    [McpServerTool(ReadOnly = true), Description("Gets all registered event stores. Returns a list of event store names.")]
    public static async Task<IEnumerable<string>> GetEventStores(ChronicleApiClient chronicleApiClient) =>
        await chronicleApiClient.GetEventStores();

    /// <summary>
    /// Gets all event sequences for a specific event store.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store to get event sequences from.</param>
    /// <returns>A collection of event sequence names.</returns>
    [McpServerTool(ReadOnly = true), Description("Gets all event sequences for a specific event store. Returns a list of event sequence names (e.g. 'event-log').")]
    public static async Task<IEnumerable<string>> GetEventSequences(ChronicleApiClient chronicleApiClient, string eventStore) =>
        await chronicleApiClient.GetEventSequences(eventStore);
}
