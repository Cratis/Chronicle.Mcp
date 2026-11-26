// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Mcp.Authorization;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools;

public class when_discovering_list_identities : Specification
{
    ServiceProvider _provider;
    CatalogTool _entry;
    McpServerTool _tool;
    JsonElement _parameters;

    void Establish() => _provider = new ServiceCollection()
        .AddSingleton(Substitute.For<IServices>())
        .AddSingleton(new ChronicleConnectionConfiguration(Options.Create(new Configuration.McpServerOptions { UseCliConfiguration = false })))
        .BuildServiceProvider();

    void Because()
    {
        _entry = new ToolCatalog(typeof(IdentityTools).Assembly).Tools.Single(tool => tool.Name == "list_identities");
        _tool = McpServerTool.Create(_entry.Method, target: null, new() { Services = _provider });
        _parameters = _tool.ProtocolTool.InputSchema.GetProperty("properties");
    }

    [Fact] void should_be_discovered_as_read_only() => _entry.IsReadOnly.ShouldBeTrue();
    [Fact] void should_advertise_read_only() => _tool.ProtocolTool.Annotations.ReadOnlyHint.ShouldEqual(true);
    [Fact] void should_advertise_a_closed_world() => _tool.ProtocolTool.Annotations.OpenWorldHint.ShouldEqual(false);
    [Fact] void should_publish_the_expected_tool_name() => _tool.ProtocolTool.Name.ShouldEqual("list_identities");
    [Fact] void should_only_publish_the_three_wire_parameters() => _parameters.EnumerateObject().Count().ShouldEqual(3);
    [Fact] void should_expose_event_store() => _parameters.TryGetProperty("eventStore", out _).ShouldBeTrue();
    [Fact] void should_expose_namespace() => _parameters.TryGetProperty("namespace", out _).ShouldBeTrue();
    [Fact] void should_expose_limit() => _parameters.TryGetProperty("limit", out _).ShouldBeTrue();

    void Destroy() => _provider.Dispose();
}
