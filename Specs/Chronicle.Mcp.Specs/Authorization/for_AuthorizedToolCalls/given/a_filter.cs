// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.given;

public class a_filter : Specs.given.a_policy
{
    protected IServices _services;
    protected AuthorizedToolCalls _calls;
    protected ServiceProvider _provider;
    protected CallToolResult _result;

    void Establish()
    {
        _services = Substitute.For<IServices>();
        _provider = new ServiceCollection().AddSingleton(_services).AddSingleton(_configuration).BuildServiceProvider();
        _calls = new(_catalog, new(_policy, _configuration, _clock), _monitor, NullLogger<AuthorizedToolCalls>.Instance);
    }

    protected ValueTask<CallToolResult> Call(string operation)
    {
        var method = _catalog.Find(operation).Method;
        var tool = McpServerTool.Create(method, (object?)null, new() { Services = _provider });
        var request = new RequestContext<CallToolRequestParams>(Substitute.For<McpServer>(), new() { Method = RequestMethods.ToolsCall }, new() { Name = operation, Arguments = _arguments }) { Services = _provider };

        return _calls.Filter(tool.InvokeAsync)(request, CancellationToken.None);
    }

    void Destroy() => _provider.Dispose();
}
