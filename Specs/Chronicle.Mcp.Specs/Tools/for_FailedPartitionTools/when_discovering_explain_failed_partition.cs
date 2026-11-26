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

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools;

public class when_discovering_explain_failed_partition : Specification
{
    const string ToolName = "explain_failed_partition";

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
        var catalog = new ToolCatalog(typeof(FailedPartitionTools).Assembly);
        _entry = catalog.ForProfile(DeploymentProfile.ReadOnly).Single(tool => tool.Name == ToolName);
        _tool = McpServerTool.Create(_entry.Method, target: null, new() { Services = _provider });
        _parameters = _tool.ProtocolTool.InputSchema.GetProperty("properties");
    }

    [Fact] void should_advertise_read_only() => _tool.ProtocolTool.Annotations.ReadOnlyHint.ShouldEqual(true);
    [Fact] void should_advertise_a_closed_world() => _tool.ProtocolTool.Annotations.OpenWorldHint.ShouldEqual(false);
    [Fact] void should_publish_the_expected_tool_name() => _tool.ProtocolTool.Name.ShouldEqual(ToolName);
    [Fact] void should_only_publish_the_five_wire_parameters() => _parameters.EnumerateObject().Select(property => property.Name).Order().ShouldEqual([
        "eventSequenceId", "eventStore", "failedPartitionId", "namespace", "precedingCount"]);
    [Fact] void should_describe_every_parameter() => Assert.All(_parameters.EnumerateObject(), property => Assert.False(string.IsNullOrWhiteSpace(property.Value.GetProperty("description").GetString())));
    [Fact] void should_require_only_the_failure_id() => _tool.ProtocolTool.InputSchema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).ShouldEqual(["failedPartitionId"]);
    [Fact] void should_default_to_the_event_log() => _parameters.GetProperty("eventSequenceId").GetProperty("default").GetString().ShouldEqual("event-log");
    [Fact] void should_default_to_ten_predecessors() => _parameters.GetProperty("precedingCount").GetProperty("default").GetInt32().ShouldEqual(10);

    void Destroy() => _provider.Dispose();
}
