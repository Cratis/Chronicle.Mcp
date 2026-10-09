// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

using DeploymentOptions = Cratis.Chronicle.Mcp.Configuration.McpServerOptions;
using ProtocolOptions = ModelContextProtocol.Server.McpServerOptions;

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Configures profile-aware tool registration and the authorization boundary.
/// </summary>
public static class DeploymentRegistration
{
    /// <summary>
    /// Adds live options and fail-closed startup validation before the MCP hosted service.
    /// </summary>
    /// <param name="services">The host services.</param>
    /// <param name="configuration">The MCP configuration section.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDeploymentPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DeploymentOptions>().Configure(options => McpOptionsBinding.Bind(configuration, options));
        services.AddSingleton<IOptionsChangeTokenSource<DeploymentOptions>>(new ConfigurationChangeTokenSource<DeploymentOptions>(configuration));
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(new ToolCatalog(typeof(DeploymentRegistration).Assembly));
        services.AddSingleton<MutationPolicy>();
        services.AddTransient<MutationAuthorizer>();
        services.AddTransient<AuthorizedToolCalls>();
        services.AddHostedService<DeploymentProfileStartup>();

        return services;
    }

    /// <summary>
    /// Registers the startup profile's subset and a call filter for every tool invocation.
    /// </summary>
    /// <param name="builder">The MCP server builder.</param>
    /// <returns>The builder for further server configuration.</returns>
    public static IMcpServerBuilder WithDeploymentTools(this IMcpServerBuilder builder)
    {
        builder.Services.AddOptions<ProtocolOptions>().Configure<IServiceProvider, IOptions<DeploymentOptions>, ToolCatalog>((protocol, services, settings, catalog) =>
        {
            protocol.ToolCollection = new();
            foreach (var tool in catalog.ForProfile(settings.Value.Profile))
            {
                protocol.ToolCollection.Add(CreateTool(tool, services));
            }
        });
        builder.WithRequestFilters(filters => filters.AddCallToolFilter(next => (request, cancellationToken) =>
            request.Services!.GetRequiredService<AuthorizedToolCalls>().Filter(next)(request, cancellationToken)));

        return builder;
    }

    static McpServerTool CreateTool(CatalogTool tool, IServiceProvider services)
    {
        var options = new McpServerToolCreateOptions { Services = services };

        return tool.Method.IsStatic
            ? McpServerTool.Create(tool.Method, (object?)null, options)
            : McpServerTool.Create(tool.Method, request => ActivatorUtilities.GetServiceOrCreateInstance(request.Services!, tool.Method.DeclaringType!), options);
    }
}
