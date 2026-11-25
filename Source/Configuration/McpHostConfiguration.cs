// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Creates a host without discovering configuration in the working directory.
/// </summary>
public static class McpHostConfiguration
{
    /// <summary>
    /// Creates the MCP host builder using an explicit JSON file and environment variables only.
    /// </summary>
    /// <param name="args">An optional <c language="text">--config</c> argument and absolute file path.</param>
    /// <returns>The configured builder.</returns>
    public static HostApplicationBuilder CreateBuilder(string[] args)
    {
        var path = ResolvePath(args);
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { DisableDefaults = true });
        if (path is not null)
        {
            builder.Configuration.AddJsonFile(path, optional: false, reloadOnChange: false);
        }

        builder.Configuration.AddEnvironmentVariables();
        return builder;
    }

    static string? ResolvePath(string[] args)
    {
        if (args.Length != 0 && (args.Length != 2 || args[0] != "--config"))
        {
            throw new ArgumentException("Use --config followed by an absolute configuration file path.", nameof(args));
        }

        var path = args.Length == 2 ? args[1] : Environment.GetEnvironmentVariable("CHRONICLE_MCP_CONFIG");
        if (path is not null && !Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("The MCP configuration file path must be absolute.", nameof(args));
        }

        return path;
    }
}
