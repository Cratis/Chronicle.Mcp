// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
    /// Unlike the stock <c>AddCratisChronicleConnection</c> extension, this registration wires up an
    /// authenticating token provider (OAuth client credentials or a static API key) based on the
    /// resolved connection string, so the server can authenticate against secured Chronicle servers.
    /// Tokens are cached on disk in the same location as the Cratis CLI for interoperability.
    /// </remarks>
    public static IServiceCollection AddChronicleMcpConnection(this IServiceCollection services)
    {
        services.AddSingleton<ChronicleConnectionConfiguration>();
        services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();
        services.AddSingleton(CreateConnection);
        services.AddSingleton(sp => ((IChronicleServicesAccessor)sp.GetRequiredService<IChronicleConnection>()).Services);

        return services.AddCratisChronicleServices();
    }

    static IChronicleConnection CreateConnection(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<ChronicleConnectionConfiguration>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var lifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
        var correlationIdAccessor = serviceProvider.GetRequiredService<ICorrelationIdAccessor>();

        var connectionString = new ChronicleConnectionString(configuration.ResolveConnectionString());
        var managementPort = configuration.ResolveManagementPort();

        // Ownership of the token provider is transferred to the ChronicleConnection, which disposes it when it is disposed.
#pragma warning disable CA2000
        var tokenProvider = CreateTokenProvider(connectionString, managementPort, configuration.ContextName, loggerFactory);
#pragma warning restore CA2000

        var connectTimeoutSeconds = int.TryParse(Environment.GetEnvironmentVariable("CHRONICLE_CONNECT_TIMEOUT_SECONDS"), out var timeout) ? timeout : 5;

        return new ChronicleConnection(
            connectionString,
            connectTimeout: connectTimeoutSeconds,
            maxReceiveMessageSize: null,
            maxSendMessageSize: null,
            new ConnectionLifecycle(loggerFactory.CreateLogger<ConnectionLifecycle>()),
            new Tasks.TaskFactory(),
            correlationIdAccessor,
            loggerFactory,
            lifetime.ApplicationStopping,
            loggerFactory.CreateLogger<ChronicleConnection>(),
            connectionString.DisableTls,
            connectionString.CertificatePath,
            connectionString.CertificatePassword,
            tokenProvider,
            skipCompatibilityCheck: true,
            skipKeepAlive: false);
    }

    static ITokenProvider? CreateTokenProvider(ChronicleConnectionString connectionString, int managementPort, string contextName, ILoggerFactory loggerFactory) =>
        connectionString.AuthenticationMode switch
        {
            AuthenticationMode.ClientCredentials => CreateCachingTokenProvider(connectionString, managementPort, contextName, loggerFactory),
            AuthenticationMode.ApiKey when !string.IsNullOrEmpty(connectionString.ApiKey) => new StaticTokenProvider(connectionString.ApiKey),
            _ => null
        };

    static FileSystemCachingTokenProvider CreateCachingTokenProvider(ChronicleConnectionString connectionString, int managementPort, string contextName, ILoggerFactory loggerFactory)
    {
        var cachePath = ChronicleCliConfiguration.GetTokenCachePath($"{contextName}_{connectionString.Username ?? string.Empty}");
        Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);

        // Ownership of the inner provider is transferred to the FileSystemCachingTokenProvider, which disposes it.
#pragma warning disable CA2000
        var inner = new OAuthTokenProvider(
            connectionString.ServerAddress,
            connectionString.Username ?? string.Empty,
            connectionString.Password ?? string.Empty,
            managementPort,
            connectionString.DisableTls,
            loggerFactory.CreateLogger<OAuthTokenProvider>());
#pragma warning restore CA2000

        return new FileSystemCachingTokenProvider(inner, cachePath);
    }
}
