// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Tools.Design.Conformance;
using Cratis.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Connection;

/// <summary>
/// Extension methods for registering the Chronicle connection used by the MCP server.
/// </summary>
public static class ChronicleMcpServiceCollectionExtensions
{
    /// <summary>
    /// Adds a Chronicle connection and its services to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for continuation.</returns>
    /// <remarks>
    /// Unlike the stock <c language="csharp">AddCratisChronicleConnection</c> extension, this registration wires up an
    /// authenticating token provider (OAuth client credentials or a static API key) based on the
    /// resolved connection string, so the server can authenticate against secured Chronicle servers.
    /// Tokens for fixed destinations are cached in a separate MCP-owned cache and validated against that destination.
    /// Explicit OAuth connections using SRV, multiple hosts, or a load-balancer override do not use the disk cache.
    /// </remarks>
    public static IServiceCollection AddChronicleMcpConnection(this IServiceCollection services)
    {
        services.AddSingleton<ChronicleConnectionConfiguration>();
        services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();
        services.AddSingleton<IChronicleConnection>(sp => CreateConnection(sp, audit: false));
        services.AddSingleton(sp => ((IChronicleServicesAccessor)sp.GetRequiredService<IChronicleConnection>()).Services);
        services.AddKeyedSingleton<IGetServicesAsynchronously>(ConformanceConnection.Key, (sp, _) => CreateConnection(sp, audit: true));
        services.AddSingleton(_ => new ConformanceConnection());
        CatalogStartup.Register(services);

        return services.AddCratisChronicleServices();
    }

    static ChronicleConnection CreateConnection(IServiceProvider serviceProvider, bool audit)
    {
        var configuration = serviceProvider.GetRequiredService<ChronicleConnectionConfiguration>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var lifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
        var correlationIdAccessor = serviceProvider.GetRequiredService<ICorrelationIdAccessor>();

        var connectionString = new ChronicleConnectionString(configuration.ResolveConnectionString());
        MutationCredentialPolicy.ValidateConnection(connectionString, serviceProvider.GetRequiredService<IOptions<McpServerOptions>>().Value);

        // Ownership of the token provider is transferred to the ChronicleConnection, which disposes it when it is disposed.
#pragma warning disable CA2000
        var tokenProvider = CreateTokenProvider(connectionString, configuration.ContextName, loggerFactory, useDiskCache: !audit);
#pragma warning restore CA2000

        var connectTimeoutSeconds = int.TryParse(Environment.GetEnvironmentVariable("CHRONICLE_CONNECT_TIMEOUT_SECONDS"), out var timeout) ? timeout : 5;

        return new ChronicleConnection(
            connectionString,
            connectTimeout: connectTimeoutSeconds,
            maxReceiveMessageSize: audit ? ConformanceConnection.MaxReceiveBytes : null,
            maxSendMessageSize: null,
            new ConnectionLifecycle(loggerFactory.CreateLogger<ConnectionLifecycle>()),
            new Tasks.TaskFactory(),
            correlationIdAccessor,
            loggerFactory,
            lifetime.ApplicationStopping,
            loggerFactory.CreateLogger<ChronicleConnection>(),
            connectionString.SkipTlsValidation,
            connectionString.CertificatePath,
            connectionString.CertificatePassword,
            tokenProvider,
            skipCompatibilityCheck: true,
            skipKeepAlive: audit);
    }

    static ITokenProvider? CreateTokenProvider(ChronicleConnectionString connectionString, string contextName, ILoggerFactory loggerFactory, bool useDiskCache) =>
        connectionString.AuthenticationMode switch
        {
            AuthenticationMode.ClientCredentials => CreateOAuthTokenProvider(connectionString, contextName, loggerFactory, useDiskCache),
            AuthenticationMode.ApiKey when !string.IsNullOrEmpty(connectionString.ApiKey) => new StaticTokenProvider(connectionString.ApiKey),
            _ => null
        };

    static ITokenProvider CreateOAuthTokenProvider(ChronicleConnectionString connectionString, string contextName, ILoggerFactory loggerFactory, bool useDiskCache)
    {
        // Ownership of the inner provider is transferred to the connection or destination-bound provider, which disposes it.
        // The token endpoint is derived from the connection's server address (host:port/connect/token),
        // so there is no separate management-port argument as in earlier Chronicle client releases.
#pragma warning disable CA2000
        var inner = new OAuthTokenProvider(
            connectionString.ServerAddress,
            connectionString.Username ?? string.Empty,
            connectionString.Password ?? string.Empty,
            connectionString.SkipTlsValidation,
            loggerFactory.CreateLogger<OAuthTokenProvider>());
#pragma warning restore CA2000

        if (!useDiskCache || !ChronicleDestination.IsFixed(connectionString))
        {
            return inner;
        }

        var destination = ChronicleDestination.ForTokenCache(connectionString);
        var cachePath = McpTokenCache.GetPath(destination, contextName, connectionString.Username ?? string.Empty);
        return new DestinationTokenProvider(inner, cachePath, destination);
    }
}
