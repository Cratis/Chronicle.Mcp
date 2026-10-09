// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using Cratis.Chronicle.Mcp.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.given;

public class a_bound_policy : Authorization.Specs.given.a_policy
{
    protected IConfigurationRoot _root;
    protected ServiceProvider _bindingProvider;
    protected Exception? _error;

    void Establish()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_settings)));
        var json = new ConfigurationBuilder().AddJsonStream(stream).Build();
        _root = new ConfigurationBuilder().AddInMemoryCollection(json.AsEnumerable().Where(setting => setting.Key.Length > 0)).Build();
        _bindingProvider = new ServiceCollection().AddSingleton(_clock).AddDeploymentPolicy(_root).BuildServiceProvider();
        _monitor = _bindingProvider.GetRequiredService<IOptionsMonitor<McpServerOptions>>();
    }

    protected Task Start() => _bindingProvider.GetServices<IHostedService>().Single().StartAsync(CancellationToken.None);

    void Destroy() => _bindingProvider.Dispose();
}
