// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Represents options for configuring the Chronicle MCP server.
/// </summary>
/// <remarks>
/// These options let the MCP server be configured independently of the Cratis CLI. When a value
/// here is left unset, the corresponding value is resolved from the CLI configuration at
/// <c>~/.cratis/config.json</c> (when <see cref="UseCliConfiguration"/> is enabled), then from
/// environment variables, and finally from built-in development defaults.
/// </remarks>
public class McpServerOptions
{
    /// <summary>
    /// Gets or sets the connection string for the Chronicle server.
    /// </summary>
    /// <remarks>
    /// When not set, the connection string is resolved from the <c>CHRONICLE_CONNECTION_STRING</c>
    /// environment variable, then from the active CLI context, and finally defaults to a local
    /// development server.
    /// </remarks>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the management port for the HTTP API and token endpoint.
    /// </summary>
    public int? ManagementPort { get; set; }

    /// <summary>
    /// Gets or sets the name of the CLI context to read connection details from.
    /// </summary>
    /// <remarks>
    /// When not set, the active context from <c>~/.cratis/config.json</c> is used.
    /// </remarks>
    public string? Context { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to fall back to the Cratis CLI configuration
    /// at <c>~/.cratis/config.json</c> for any value not explicitly configured here.
    /// </summary>
    public bool UseCliConfiguration { get; set; } = true;

    /// <summary>
    /// Gets or sets the client id used for client credentials authentication.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret used for client credentials authentication.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets an API key to authenticate with, as an alternative to client credentials.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the default event store used by tools when none is specified.
    /// </summary>
    public string? EventStore { get; set; }

    /// <summary>
    /// Gets or sets the default namespace used by tools when none is specified.
    /// </summary>
    public string? Namespace { get; set; }
}
