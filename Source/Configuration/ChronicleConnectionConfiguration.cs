// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Resolves explicitly configured Chronicle settings without discovering files in the working directory.
/// </summary>
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
    /// Environment variable holding the Chronicle connection string.
    /// </summary>
    public const string ConnectionStringEnvVar = "CHRONICLE_CONNECTION_STRING";

    readonly McpServerOptions _options = options.Value;
    readonly ChronicleCliConfiguration _configuration = LoadCliConfiguration(options.Value);

    /// <summary>
    /// Gets the selected CLI context name.
    /// </summary>
    public string ContextName => _options.Context ?? _configuration.ActiveContextName;

    ChronicleCliContext Context => _configuration.GetContext(_options.Context);

    /// <summary>
    /// Resolves the connection string and credentials, refusing implicit authentication defaults.
    /// </summary>
    /// <returns>The resolved connection string.</returns>
    public string ResolveConnectionString()
    {
        var connectionString = FirstNonEmpty(
            _options.ConnectionString,
            Environment.GetEnvironmentVariable(ConnectionStringEnvVar),
            Context.Server);
        connectionString ??= _options.UseDevelopmentCredentials
            ? "chronicle://localhost:35000"
            : throw new InvalidOperationException("Configure a Chronicle ConnectionString explicitly, or enable UseDevelopmentCredentials for local development.");

        return ComposeCredentials(new ChronicleConnectionString(connectionString));
    }

    /// <summary>
    /// Resolves the default event store.
    /// </summary>
    /// <returns>The selected event store.</returns>
    public string ResolveEventStore() =>
        FirstNonEmpty(_options.EventStore, Context.EventStore) ?? DefaultEventStoreName;

    /// <summary>
    /// Resolves the default namespace.
    /// </summary>
    /// <returns>The selected namespace.</returns>
    public string ResolveNamespace() =>
        FirstNonEmpty(_options.Namespace, Context.Namespace) ?? DefaultNamespaceName;

    static ChronicleCliConfiguration LoadCliConfiguration(McpServerOptions options)
    {
        if (!options.UseCliConfiguration)
        {
            return new ChronicleCliConfiguration();
        }

        if (string.IsNullOrWhiteSpace(options.CliConfigurationPath))
        {
            throw new InvalidOperationException("UseCliConfiguration requires an absolute CliConfigurationPath.");
        }

        return ChronicleCliConfiguration.Load(options.CliConfigurationPath);
    }

    static bool IsTokenValid(string? tokenExpiry) =>
        DateTimeOffset.TryParse(tokenExpiry, out var expiry) && expiry > DateTimeOffset.UtcNow.AddMinutes(1);

    static bool HasCredentials(string? clientId, string? clientSecret) =>
        !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret);

    static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    string ComposeCredentials(ChronicleConnectionString connectionString)
    {
        var destination = ChronicleDestination.For(connectionString);
        var explicitCredentials = ResolveExplicitCredentials(connectionString);
        if (explicitCredentials is not null)
        {
            return explicitCredentials;
        }

        if (CanUseLogin(destination))
        {
            return connectionString.WithApiKey(Context.AccessToken!).ToString();
        }

        if (CanUseContextCredentials(destination))
        {
            return connectionString.WithCredentials(Context.ClientId!, Context.ClientSecret!).ToString();
        }

        if (_options.UseDevelopmentCredentials)
        {
            return connectionString.WithCredentials(ChronicleConnectionString.DevelopmentClient, ChronicleConnectionString.DevelopmentClientSecret).ToString();
        }

        throw new InvalidOperationException("Configure Chronicle credentials explicitly. CLI login tokens require a matching tokenServer; development credentials require UseDevelopmentCredentials=true.");
    }

    string? ResolveExplicitCredentials(ChronicleConnectionString connectionString)
    {
        if (connectionString.AuthenticationMode != AuthenticationMode.None)
        {
            return connectionString.ToString();
        }

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return connectionString.WithApiKey(_options.ApiKey).ToString();
        }

        if (HasCredentials(_options.ClientId, _options.ClientSecret))
        {
            return connectionString.WithCredentials(_options.ClientId!, _options.ClientSecret!).ToString();
        }

        if (FirstNonEmpty(_options.ClientId, _options.ClientSecret, connectionString.Username, connectionString.Password) is not null)
        {
            throw new InvalidOperationException("Chronicle client credentials require both ClientId and ClientSecret.");
        }

        return null;
    }

    bool CanUseLogin(string destination) =>
        !string.IsNullOrWhiteSpace(Context.AccessToken) && IsTokenValid(Context.TokenExpiry) &&
        string.Equals(Context.TokenServer, destination, StringComparison.OrdinalIgnoreCase);

    bool CanUseContextCredentials(string destination) =>
        HasCredentials(Context.ClientId, Context.ClientSecret) && !string.IsNullOrWhiteSpace(Context.Server) &&
        string.Equals(ChronicleDestination.For(new ChronicleConnectionString(Context.Server)), destination, StringComparison.Ordinal);
}
