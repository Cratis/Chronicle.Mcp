// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Helpers for resolving common tool inputs against the configured defaults.
/// </summary>
public static class ToolContext
{
    /// <summary>
    /// Resolves the event store to use, falling back to the configured default when none is supplied.
    /// </summary>
    /// <param name="configuration">The <see cref="ChronicleConnectionConfiguration"/> holding the defaults.</param>
    /// <param name="eventStore">The optional event store provided by the caller.</param>
    /// <returns>The resolved event store name.</returns>
    public static string ResolveEventStore(this ChronicleConnectionConfiguration configuration, string? eventStore) =>
        string.IsNullOrWhiteSpace(eventStore) ? configuration.ResolveEventStore() : eventStore;

    /// <summary>
    /// Resolves the namespace to use, falling back to the configured default when none is supplied.
    /// </summary>
    /// <param name="configuration">The <see cref="ChronicleConnectionConfiguration"/> holding the defaults.</param>
    /// <param name="namespace">The optional namespace provided by the caller.</param>
    /// <returns>The resolved namespace name.</returns>
    public static string ResolveNamespace(this ChronicleConnectionConfiguration configuration, string? @namespace) =>
        string.IsNullOrWhiteSpace(@namespace) ? configuration.ResolveNamespace() : @namespace;
}
