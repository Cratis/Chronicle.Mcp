// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Mcp.Api;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp;

/// <summary>
/// Represents a set of tools for querying identities from Chronicle.
/// </summary>
[McpServerToolType]
public static class IdentityTools
{
    /// <summary>
    /// Gets all identities that have caused events in a specific event store and namespace.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store to get identities from.</param>
    /// <param name="namespace">The namespace to query. Defaults to 'Default'.</param>
    /// <returns>A collection of <see cref="Identity"/> with subject, name, username, and delegation info.</returns>
    [McpServerTool(ReadOnly = true), Description("Gets all identities that have caused events in a specific event store and namespace. Each identity includes subject, name, username, and optional on-behalf-of delegation chain.")]
    public static async Task<IEnumerable<Identity>> GetIdentities(
        ChronicleApiClient chronicleApiClient,
        string eventStore,
        string? @namespace = "Default") =>
        await chronicleApiClient.GetIdentities(eventStore, @namespace ?? "Default");
}
