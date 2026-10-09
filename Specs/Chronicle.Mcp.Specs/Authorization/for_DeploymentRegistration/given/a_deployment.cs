// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

using ProtocolOptions = ModelContextProtocol.Server.McpServerOptions;

namespace Cratis.Chronicle.Mcp.Authorization.for_DeploymentRegistration.given;

public class a_deployment : Specs.given.a_policy
{
    protected string _profile = "ReadOnly";
    protected ServiceProvider _provider;
    protected ProtocolOptions _protocol;

    protected void Register()
    {
        var section = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Profile"] = _profile,
            ["UseCliConfiguration"] = "false"
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.ClearProviders());
        services.AddSingleton(_configuration);
        services.AddDeploymentPolicy(section);
        services.AddMcpServer().WithDeploymentTools();
        _provider = services.BuildServiceProvider();
        _protocol = _provider.GetRequiredService<IOptions<ProtocolOptions>>().Value;
    }

    void Destroy() => _provider.Dispose();
}
