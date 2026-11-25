// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Represents options for configuring the Chronicle MCP server.
/// </summary>
/// <remarks>
/// Explicit options take precedence over the connection environment variable and an explicitly
/// selected CLI configuration. Development credentials require <see cref="UseDevelopmentCredentials"/>.
/// </remarks>
public class McpServerOptions
{
    /// <summary>
    /// Gets or sets the deployment profile. Defaults to read-only.
    /// </summary>
    public DeploymentProfile Profile { get; set; } = DeploymentProfile.ReadOnly;

    /// <summary>
    /// Gets or sets the policy required by the mutation profile.
    /// </summary>
    public MutationAuthorization Authorization { get; set; } = new();

    /// <summary>
    /// Gets or sets the connection string for the Chronicle server.
    /// </summary>
    /// <remarks>
    /// When not set, the connection string is resolved from the <c language="csharp">CHRONICLE_CONNECTION_STRING</c>
    /// environment variable, then from the selected CLI context. A local server default requires
    /// <see cref="UseDevelopmentCredentials"/>.
    /// </remarks>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the CLI context to read connection details from.
    /// </summary>
    /// <remarks>
    /// When not set, the active context from <c language="csharp">~/.cratis/config.json</c> is used.
    /// </remarks>
    public string? Context { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to read the explicitly selected CLI configuration.
    /// Defaults to false.
    /// </summary>
    public bool UseCliConfiguration { get; set; }

    /// <summary>
    /// Gets or sets the absolute CLI configuration file path. Required when CLI configuration is enabled.
    /// </summary>
    public string? CliConfigurationPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use built-in local development credentials and server defaults.
    /// Defaults to false.
    /// </summary>
    public bool UseDevelopmentCredentials { get; set; }

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
