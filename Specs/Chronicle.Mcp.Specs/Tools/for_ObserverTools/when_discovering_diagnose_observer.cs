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

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools;

public class when_discovering_diagnose_observer : Specification
{
    const string ToolName = "diagnose_observer";

    ServiceProvider _provider;
    McpServerTool _tool;
    JsonElement _parameters;

    void Establish() => _provider = new ServiceCollection()
        .AddSingleton(Substitute.For<IServices>())
        .AddSingleton(new ChronicleConnectionConfiguration(Options.Create(new Configuration.McpServerOptions { UseCliConfiguration = false })))
        .BuildServiceProvider();

    void Because()
    {
        var catalog = new ToolCatalog(typeof(ObserverTools).Assembly);
        var entry = catalog.ForProfile(DeploymentProfile.ReadOnly).Single(tool => tool.Name == ToolName);
        _tool = McpServerTool.Create(entry.Method, target: null, new() { Services = _provider });
        _parameters = _tool.ProtocolTool.InputSchema.GetProperty("properties");
    }

    [Fact] void should_advertise_read_only() => _tool.ProtocolTool.Annotations.ReadOnlyHint.ShouldEqual(true);
    [Fact] void should_advertise_a_closed_world() => _tool.ProtocolTool.Annotations.OpenWorldHint.ShouldEqual(false);
    [Fact] void should_publish_the_expected_name() => _tool.ProtocolTool.Name.ShouldEqual(ToolName);
    [Fact] void should_only_publish_the_four_wire_parameters() => _parameters.EnumerateObject().Select(property => property.Name).Order().ShouldEqual([
        "eventSequenceId", "eventStore", "namespace", "observerId"]);
    [Fact] void should_describe_every_parameter() => Assert.All(_parameters.EnumerateObject(), property => Assert.False(string.IsNullOrWhiteSpace(property.Value.GetProperty("description").GetString())));
    [Fact] void should_require_only_the_observer_id() => _tool.ProtocolTool.InputSchema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).ShouldEqual(["observerId"]);
    [Fact] void should_default_to_the_event_log() => _parameters.GetProperty("eventSequenceId").GetProperty("default").GetString().ShouldEqual("event-log");

    void Destroy() => _provider.Dispose();
}
