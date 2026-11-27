// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Mcp.Authorization;
using Cratis.Chronicle.Mcp.Tools;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.given;

public class a_failed_partition_tool_call : a_failed_partition_query
{
    protected readonly CancellationTokenSource _cancellation = new();
    ServiceProvider _provider;

    void Establish() => _provider = new ServiceCollection().AddSingleton(_services).AddSingleton(_configuration).BuildServiceProvider();

    protected async Task<CallToolResult> Invoke(string name, IDictionary<string, JsonElement>? arguments = null)
    {
        var method = new ToolCatalog(typeof(FailedPartitionTools).Assembly).Find(name)!.Method;
        var tool = McpServerTool.Create(method, target: null, new() { Services = _provider });
        var request = new RequestContext<CallToolRequestParams>(
            Substitute.For<McpServer>(),
            new() { Method = RequestMethods.ToolsCall },
            new() { Name = name, Arguments = arguments })
        {
            Services = _provider
        };
        return await tool.InvokeAsync(request, _cancellation.Token);
    }

    void Destroy()
    {
        _provider.Dispose();
        _cancellation.Dispose();
    }
}
