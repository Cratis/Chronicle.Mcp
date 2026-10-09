// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.given;

public class a_layered_filter : Configuration.for_McpOptionsBinding.given.a_layered_policy
{
    protected IServices _services;
    protected AuthorizedToolCalls _calls;
    protected CallToolResult _firstResult;
    protected CallToolResult _result;
    protected Exception? _reloadError;

    ServiceProvider _toolProvider;

    void Establish()
    {
        _services = Substitute.For<IServices>();
        _toolProvider = new ServiceCollection().AddSingleton(_services).AddSingleton(_configuration).BuildServiceProvider();
        _calls = new(_catalog, new(_policy, _configuration, _clock), _monitor, NullLogger<AuthorizedToolCalls>.Instance);
    }

    protected ValueTask<CallToolResult> Call()
    {
        var tool = McpServerTool.Create(_catalog.Find("stop_job").Method, (object?)null, new() { Services = _toolProvider });
        var request = new RequestContext<CallToolRequestParams>(Substitute.For<McpServer>(), new() { Method = RequestMethods.ToolsCall }, new() { Name = "stop_job", Arguments = _arguments }) { Services = _toolProvider, MatchedPrimitive = tool };

        return _calls.Filter(tool.InvokeAsync)(request, CancellationToken.None);
    }

    void Destroy() => _toolProvider.Dispose();
}
