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

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_discovering_step_projection : Specification
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
        _entry = new ToolCatalog(typeof(ProjectionStepTools).Assembly).Tools.Single(tool => tool.Name == "step_projection");
        _tool = McpServerTool.Create(_entry.Method, target: null, new() { Services = _provider });
        _parameters = _tool.ProtocolTool.InputSchema.GetProperty("properties");
    }

    [Fact] void should_be_discovered_as_read_only() => _entry.IsReadOnly.ShouldBeTrue();
    [Fact] void should_have_no_authorization_targets() => _entry.Method.GetParameters().Where(parameter => Attribute.IsDefined(parameter, typeof(AuthorizationTargetAttribute))).ShouldBeEmpty();
    [Fact] void should_advertise_read_only() => _tool.ProtocolTool.Annotations.ReadOnlyHint.ShouldEqual(true);
    [Fact] void should_advertise_a_closed_world() => _tool.ProtocolTool.Annotations.OpenWorldHint.ShouldEqual(false);
    [Fact] void should_only_publish_the_six_wire_parameters() => _parameters.EnumerateObject().Count().ShouldEqual(6);
    [Fact] void should_expose_read_model() => _parameters.TryGetProperty("readModel", out _).ShouldBeTrue();
    [Fact] void should_expose_read_model_key() => _parameters.TryGetProperty("readModelKey", out _).ShouldBeTrue();
    [Fact] void should_expose_event_sequence_id() => _parameters.TryGetProperty("eventSequenceId", out _).ShouldBeTrue();
    [Fact] void should_expose_max_events() => _parameters.TryGetProperty("maxEvents", out _).ShouldBeTrue();
    [Fact] void should_not_expose_cancellation() => _parameters.TryGetProperty("cancellationToken", out _).ShouldBeFalse();

    void Destroy() => _provider.Dispose();
}
