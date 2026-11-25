// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cratis.Chronicle.Mcp.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.given;

public class a_layered_policy : Authorization.Specs.given.a_policy
{
    protected JsonObject _json;
    protected ReloadableJsonConfiguration _jsonProvider;
    protected ReloadableMemoryConfiguration _overlay;
    protected IConfigurationRoot _root;
    protected ServiceProvider _bindingProvider;
    protected Exception? _error;
    protected ILogger<MutationPolicyWarnings> _logger;

    void Establish()
    {
        _settings.ConnectionString = "chronicle://localhost:35000";
        _settings.ClientId = "spec-client";
        _settings.ClientSecret = "spec-secret";
        _json = JsonSerializer.SerializeToNode(_settings).AsObject();
        _json["Authorization"].AsObject().Remove("Allowlist");
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(_json.ToJsonString()));
        _jsonProvider = new(new() { Stream = stream });
        _overlay = new();
        _root = new ConfigurationBuilder().Add(_jsonProvider).Add(_overlay).Build();
        _logger = Substitute.For<ILogger<MutationPolicyWarnings>>();
        _logger.IsEnabled(LogLevel.Warning).Returns(true);
        _bindingProvider = new ServiceCollection().AddSingleton(_logger).AddSingleton(_clock).AddDeploymentPolicy(_root).BuildServiceProvider();
        _monitor = _bindingProvider.GetRequiredService<IOptionsMonitor<McpServerOptions>>();
    }

    protected void WithEmptyArray(string path)
    {
        var segments = path.Split(':');
        JsonNode node = _json;
        foreach (var segment in segments[..^1])
        {
            node = node is JsonArray array ? array[int.Parse(segment, CultureInfo.InvariantCulture)] : node[segment];
        }

        node[segments[^1]] = new JsonArray();
        _jsonProvider.Replace(_json.ToJsonString());
    }

    protected void WithReadOnlyProfile()
    {
        _json["Profile"] = "ReadOnly";
        _jsonProvider.Replace(_json.ToJsonString());
    }

    protected Task Start() => _bindingProvider.GetServices<IHostedService>().Single().StartAsync(CancellationToken.None);

    void Destroy()
    {
        _bindingProvider.Dispose();
        ((IDisposable)_root).Dispose();
    }
}
