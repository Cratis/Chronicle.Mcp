// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Connection;

/// <summary>
/// Locates MCP-owned token caches, separate from the CLI's unscoped cache.
/// </summary>
public static class McpTokenCache
{
    /// <summary>
    /// Gets the cache path for a destination, context and client identity.
    /// </summary>
    /// <param name="destination">The normalized server endpoint.</param>
    /// <param name="contextName">The context name.</param>
    /// <param name="clientId">The client identity.</param>
    /// <returns>The MCP-owned cache path.</returns>
    public static string GetPath(string destination, string contextName, string clientId)
    {
        var key = JsonSerializer.Serialize(new[] { destination, contextName, clientId });
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cratis", "mcp", "tokens", $"{hash}.json");
    }
}
