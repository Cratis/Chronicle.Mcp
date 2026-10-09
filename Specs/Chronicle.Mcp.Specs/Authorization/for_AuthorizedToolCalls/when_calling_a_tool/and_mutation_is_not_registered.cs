// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_mutation_is_not_registered : given.a_filter
{
    Exception? _error;
    McpProtocolException _unknownTool;

    void Establish()
    {
        _settings.Profile = DeploymentProfile.ReadOnly;
        _unknownTool = new("Unknown tool: 'stop_job'", McpErrorCode.InvalidParams);
    }

    async Task Because()
    {
        var request = new RequestContext<CallToolRequestParams>(Substitute.For<McpServer>(), new() { Method = RequestMethods.ToolsCall }, new() { Name = "stop_job", Arguments = _arguments }) { Services = _provider };
        var handler = _calls.Filter((_, _) => ValueTask.FromException<CallToolResult>(_unknownTool));
        _error = await Catch.Exception(async () => await handler(request, CancellationToken.None));
    }

    [Fact] void should_preserve_the_sdk_unknown_tool_error() => _error.ShouldEqual(_unknownTool);
    [Fact] void should_not_read_the_mutation_policy() => _ = _monitor.DidNotReceive().CurrentValue;
}
