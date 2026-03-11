// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Mcp.Api;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp;

/// <summary>
/// Represents a set of tools for working with observers in Chronicle.
/// </summary>
[McpServerToolType]
public static class ObserverTools
{
    /// <summary>
    /// Gets all observers for a specific event store.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store to get observers from.</param>
    /// <returns>A collection of <see cref="ObserverInformation"/> for the specified event store.</returns>
    [McpServerTool(ReadOnly = true), Description("Gets all observers for a specific event store and optional namespace.")]
    public static Task<IEnumerable<ObserverInformation>> GetObservers(ChronicleApiClient chronicleApiClient, string eventStore) =>
        GetObserversForNamespace(chronicleApiClient, eventStore);

    /// <summary>
    /// Gets all observers for a specific event store and optional namespace.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store to get observers from.</param>
    /// <param name="namespace">The namespace to filter observers by.</param>
    /// <returns>A collection of <see cref="ObserverInformation"/> for the specified event store and namespace.</returns>
    [McpServerTool(ReadOnly = true), Description("Gets all observers for a specific event store and optional namespace.")]
    public static async Task<IEnumerable<ObserverInformation>> GetObserversForNamespace(ChronicleApiClient chronicleApiClient, string eventStore, string? @namespace = "Default") =>
        await chronicleApiClient.GetObservers(eventStore, @namespace ?? "Default");

    /// <summary>
    /// Replays an observer from the beginning of its event sequence.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the observer.</param>
    /// <param name="observerId">The unique identifier of the observer to replay.</param>
    /// <returns>A confirmation message.</returns>
    [McpServerTool, Description("Replays an observer from the beginning. This reprocesses all events the observer is subscribed to. Useful when an observer's projection or reactor logic has changed and the read model needs to be rebuilt.")]
    public static async Task<string> ReplayObserver(ChronicleApiClient chronicleApiClient, string eventStore, string @namespace, string observerId)
    {
        await chronicleApiClient.ReplayObserver(eventStore, @namespace, observerId);

        return $"Observer '{observerId}' replay initiated in event store '{eventStore}', namespace '{@namespace}'.";
    }

    /// <summary>
    /// Replays a specific partition of an observer.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the observer.</param>
    /// <param name="observerId">The unique identifier of the observer.</param>
    /// <param name="partition">The partition identifier to replay.</param>
    /// <returns>A confirmation message.</returns>
    [McpServerTool, Description("Replays a specific partition of an observer. Only reprocesses events for the specified partition rather than all events.")]
    public static async Task<string> ReplayObserverPartition(ChronicleApiClient chronicleApiClient, string eventStore, string @namespace, string observerId, string partition)
    {
        await chronicleApiClient.ReplayObserverPartition(eventStore, @namespace, observerId, partition);

        return $"Observer '{observerId}' partition '{partition}' replay initiated in event store '{eventStore}', namespace '{@namespace}'.";
    }

    /// <summary>
    /// Attempts to recover a failed partition for an observer.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the observer.</param>
    /// <param name="observerId">The unique identifier of the observer.</param>
    /// <param name="partition">The partition identifier to recover.</param>
    /// <returns>A confirmation message.</returns>
    [McpServerTool, Description("Attempts to recover a failed partition for an observer. Use this when a partition has failed due to a transient error and you want to retry processing from where it left off.")]
    public static async Task<string> RecoverFailedPartition(ChronicleApiClient chronicleApiClient, string eventStore, string @namespace, string observerId, string partition)
    {
        await chronicleApiClient.RecoverFailedPartition(eventStore, @namespace, observerId, partition);

        return $"Recovery of failed partition '{partition}' for observer '{observerId}' initiated in event store '{eventStore}', namespace '{@namespace}'.";
    }
}
