// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Mcp.Authorization;
using Cratis.Chronicle.Mcp.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools;

public class when_discovering_get_patterns : Specification
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
        _entry = new ToolCatalog(typeof(PatternTools).Assembly).Tools.Single(tool => tool.Name == "get_patterns");
        _tool = McpServerTool.Create(_entry.Method, target: null, new() { Services = _provider });
        _parameters = _tool.ProtocolTool.InputSchema.GetProperty("properties");
    }

    [Fact] void should_be_discovered_as_read_only() => _entry.IsReadOnly.ShouldBeTrue();
    [Fact] void should_have_no_authorization_targets() => _entry.Method.GetParameters().Where(parameter => Attribute.IsDefined(parameter, typeof(AuthorizationTargetAttribute))).ShouldBeEmpty();
    [Fact] void should_publish_the_expected_tool_name() => _tool.ProtocolTool.Name.ShouldEqual("get_patterns");
    [Fact] void should_advertise_read_only() => _tool.ProtocolTool.Annotations.ReadOnlyHint.ShouldEqual(true);
    [Fact] void should_only_publish_the_six_wire_parameters() => _parameters.EnumerateObject().Count().ShouldEqual(6);
    [Fact] void should_expose_grouping_key() => _parameters.TryGetProperty("groupingKey", out _).ShouldBeTrue();
    [Fact] void should_expose_context() => _parameters.TryGetProperty("context", out _).ShouldBeTrue();
    [Fact] void should_expose_event_store() => _parameters.TryGetProperty("eventStore", out _).ShouldBeTrue();
    [Fact] void should_expose_namespace() => _parameters.TryGetProperty("namespace", out _).ShouldBeTrue();
    [Fact] void should_expose_minimum_confidence() => _parameters.TryGetProperty("minimumConfidence", out _).ShouldBeTrue();
    [Fact] void should_expose_maximum_results() => _parameters.TryGetProperty("maximumResults", out _).ShouldBeTrue();
    [Fact] void should_not_expose_services() => _parameters.TryGetProperty("services", out _).ShouldBeFalse();
    [Fact] void should_not_expose_configuration() => _parameters.TryGetProperty("configuration", out _).ShouldBeFalse();
    [Fact] void should_not_expose_cancellation() => _parameters.TryGetProperty("cancellationToken", out _).ShouldBeFalse();

    void Destroy() => _provider.Dispose();
}
