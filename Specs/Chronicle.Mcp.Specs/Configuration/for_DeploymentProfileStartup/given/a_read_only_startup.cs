// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Authorization;
using Cratis.Chronicle.Mcp.Connection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.given;

public class a_read_only_startup : Specification
{
    protected IConfigurationRoot _configuration;
    protected ServiceProvider _provider;
    protected Exception? _error;

    void Establish()
    {
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Profile"] = "ReadOnly",
            ["UseCliConfiguration"] = "true"
        }).Build();
        _provider = new ServiceCollection().AddDeploymentPolicy(_configuration).AddChronicleMcpConnection().BuildServiceProvider();
    }

    protected Task Start() => _provider.GetServices<IHostedService>().OfType<DeploymentProfileStartup>().Single().StartAsync(CancellationToken.None);

    void Destroy() => _provider.Dispose();
}
