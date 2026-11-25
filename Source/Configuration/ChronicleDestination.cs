// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Identifies the single server to which MCP authentication may be sent.
/// </summary>
public static class ChronicleDestination
{
    /// <summary>
    /// Gets the normalized host and port, matching the CLI login destination format.
    /// </summary>
    /// <param name="connectionString">The selected connection.</param>
    /// <returns>The destination, without credentials.</returns>
    /// <exception cref="InvalidOperationException">The connection does not identify one fixed destination.</exception>
    public static string For(ChronicleConnectionString connectionString)
    {
        if (connectionString.IsSrv || !string.IsNullOrWhiteSpace(connectionString.LoadBalancer))
        {
            throw new InvalidOperationException("MCP authentication requires a fixed destination, without SRV discovery or load-balancer overrides.");
        }

        var destinations = connectionString.ServerAddresses.Select(address =>
        {
            var host = address.Host.Trim('[', ']').ToLowerInvariant();
            return host.Contains(':', StringComparison.Ordinal) ? $"[{host}]:{address.Port}" : $"{host}:{address.Port}";
        }).Distinct(StringComparer.Ordinal).ToArray();

        if (destinations.Length != 1)
        {
            throw new InvalidOperationException("MCP authentication requires a single Chronicle destination.");
        }

        return destinations[0];
    }

    /// <summary>
    /// Determines whether tokens can be cached against one fixed destination.
    /// </summary>
    /// <param name="connectionString">The selected connection.</param>
    /// <returns>Whether the configured target identifies one fixed server.</returns>
    public static bool IsFixed(ChronicleConnectionString connectionString) =>
        !connectionString.IsSrv && string.IsNullOrWhiteSpace(connectionString.LoadBalancer) && connectionString.ServerAddresses.Count == 1;

    /// <summary>
    /// Gets the MCP cache destination, including its transport security settings.
    /// </summary>
    /// <param name="connectionString">The selected connection.</param>
    /// <returns>The destination scope, without credentials.</returns>
    public static string ForTokenCache(ChronicleConnectionString connectionString)
    {
        var scheme = connectionString.DisableTls ? "http" : "https";
        return $"{scheme}://{For(connectionString)};skipTlsValidation={connectionString.SkipTlsValidation}";
    }
}
