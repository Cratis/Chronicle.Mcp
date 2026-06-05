// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Tools for inspecting the Chronicle server itself.
/// </summary>
[McpServerToolType]
public static class ServerTools
{
    /// <summary>
    /// Gets version information from the connected Chronicle server.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <returns>The server version information.</returns>
    /// <remarks>
    /// This also doubles as a connectivity check: a successful response confirms the MCP server can
    /// reach and authenticate against the Chronicle server.
    /// </remarks>
    [McpServerTool(Name = "get_server_version")]
    [Description("Gets version information from the connected Chronicle server. Use to verify connectivity and check the server version and commit.")]
    public static async Task<ServerVersion> GetServerVersion(IServices services)
    {
        var info = await services.Server.GetVersionInfo();
        return new ServerVersion(info.Version, info.CommitSha);
    }
}
