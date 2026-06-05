// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Resolves the effective Chronicle connection settings for the MCP server.
/// </summary>
/// <remarks>
/// Resolution precedence for every value is: explicit <see cref="McpServerOptions"/> first, then the
/// matching environment variable, then the active CLI context from <c>~/.cratis/config.json</c>
/// (when <see cref="McpServerOptions.UseCliConfiguration"/> is enabled), and finally a built-in
/// development default. This keeps the MCP server compatible with the Cratis CLI while still allowing
/// it to be configured entirely on its own.
/// </remarks>
/// <param name="options">The MCP server options.</param>
public class ChronicleConnectionConfiguration(IOptions<McpServerOptions> options)
{
    /// <summary>
    /// The default event store name.
    /// </summary>
    public const string DefaultEventStoreName = "default";

    /// <summary>
    /// The default namespace name.
    /// </summary>
    public const string DefaultNamespaceName = "Default";

    /// <summary>
    /// The default management port for the HTTP API and token endpoint.
    /// </summary>
    public const int DefaultManagementPort = 8080;

    /// <summary>
    /// Environment variable holding the Chronicle connection string.
    /// </summary>
    public const string ConnectionStringEnvVar = "CHRONICLE_CONNECTION_STRING";

    /// <summary>
    /// Environment variable holding the management port override.
    /// </summary>
    public const string ManagementPortEnvVar = "CHRONICLE_MANAGEMENT_PORT";

    const string DefaultConnectionString = "chronicle://localhost:35000/?disableTls=true";
    const string Scheme = "chronicle://";

    readonly McpServerOptions _options = options.Value;

    /// <summary>
    /// Gets the name of the CLI context that connection details are resolved from.
    /// </summary>
    public string ContextName => _options.Context ?? Configuration.ActiveContextName;

    bool UseCli => _options.UseCliConfiguration;

    ChronicleCliConfiguration Configuration { get; } = options.Value.UseCliConfiguration ? ChronicleCliConfiguration.Load() : new ChronicleCliConfiguration();

    ChronicleCliContext Context => UseCli ? Configuration.GetContext(_options.Context) : new ChronicleCliContext();

    /// <summary>
    /// Resolves the effective connection string, composing in credentials when none are embedded.
    /// </summary>
    /// <returns>The resolved connection string.</returns>
    public string ResolveConnectionString()
    {
        string connectionString;

        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            connectionString = _options.ConnectionString;
        }
        else
        {
            var envVar = Environment.GetEnvironmentVariable(ConnectionStringEnvVar);
            if (!string.IsNullOrWhiteSpace(envVar))
            {
                connectionString = envVar;
            }
            else
            {
                connectionString = !string.IsNullOrWhiteSpace(Context.Server)
                    ? Context.Server!
                    : DefaultConnectionString;
            }
        }

        return ComposeCredentials(connectionString);
    }

    /// <summary>
    /// Resolves the effective management port.
    /// </summary>
    /// <returns>The resolved management port.</returns>
    public int ResolveManagementPort()
    {
        if (_options.ManagementPort.HasValue)
        {
            return _options.ManagementPort.Value;
        }

        var envVar = Environment.GetEnvironmentVariable(ManagementPortEnvVar);
        if (!string.IsNullOrWhiteSpace(envVar) && int.TryParse(envVar, out var envPort))
        {
            return envPort;
        }

        return Context.ManagementPort ?? DefaultManagementPort;
    }

    /// <summary>
    /// Resolves the default event store to use when a tool is not given one explicitly.
    /// </summary>
    /// <returns>The resolved event store name.</returns>
    public string ResolveEventStore() =>
        FirstNonEmpty(_options.EventStore, Context.EventStore) ?? DefaultEventStoreName;

    /// <summary>
    /// Resolves the default namespace to use when a tool is not given one explicitly.
    /// </summary>
    /// <returns>The resolved namespace name.</returns>
    public string ResolveNamespace() =>
        FirstNonEmpty(_options.Namespace, Context.Namespace) ?? DefaultNamespaceName;

    static bool HasEmbeddedAuth(string connectionString)
    {
        if (!connectionString.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var afterScheme = connectionString[Scheme.Length..];
        var queryStart = afterScheme.IndexOf('?', StringComparison.Ordinal);
        var hostPart = queryStart >= 0 ? afterScheme[..queryStart] : afterScheme;

        return hostPart.Contains('@', StringComparison.Ordinal) ||
               connectionString.Contains("apiKey=", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsTokenValid(string? tokenExpiry) =>
        !string.IsNullOrWhiteSpace(tokenExpiry) &&
        DateTimeOffset.TryParse(tokenExpiry, out var expiry) &&
        expiry > DateTimeOffset.UtcNow.AddMinutes(1);

    static string AppendApiKey(string connectionString, string apiKey)
    {
        var separator = connectionString.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{connectionString}{separator}apiKey={Uri.EscapeDataString(apiKey)}";
    }

    static string InsertCredentials(string connectionString, string clientId, string clientSecret)
    {
        if (!connectionString.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var encodedId = Uri.EscapeDataString(clientId);
        var encodedSecret = Uri.EscapeDataString(clientSecret);
        return $"{Scheme}{encodedId}:{encodedSecret}@{connectionString[Scheme.Length..]}";
    }

    static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

    string ComposeCredentials(string connectionString)
    {
        if (HasEmbeddedAuth(connectionString))
        {
            return connectionString;
        }

        // 1. Explicit MCP options take precedence so the server can be configured independently.
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return AppendApiKey(connectionString, _options.ApiKey);
        }

        if (!string.IsNullOrWhiteSpace(_options.ClientId) && !string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            return InsertCredentials(connectionString, _options.ClientId, _options.ClientSecret);
        }

        // 2. Cached login token from 'cratis chronicle login'.
        if (!string.IsNullOrWhiteSpace(Context.AccessToken) && IsTokenValid(Context.TokenExpiry))
        {
            return AppendApiKey(connectionString, Context.AccessToken!);
        }

        // 3. Service account credentials stored in the CLI context.
        if (!string.IsNullOrWhiteSpace(Context.ClientId) && !string.IsNullOrWhiteSpace(Context.ClientSecret))
        {
            return InsertCredentials(connectionString, Context.ClientId!, Context.ClientSecret!);
        }

        // 4. Built-in development credentials for local Chronicle servers.
        return InsertCredentials(connectionString, ChronicleConnectionString.DevelopmentClient, ChronicleConnectionString.DevelopmentClientSecret);
    }
}
