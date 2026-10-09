// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_a_malformed_policy_is_reloaded : Configuration.for_McpOptionsBinding.given.a_bound_policy
{
    IServices _services;
    ServiceProvider _toolProvider;
    AuthorizedToolCalls _calls;
    CallToolResult _firstResult;
    CallToolResult _result;
    Exception? _reloadError;

    void Establish()
    {
        _services = Substitute.For<IServices>();
        _toolProvider = new ServiceCollection().AddSingleton(_services).AddSingleton(_configuration).BuildServiceProvider();
        _calls = new(_catalog, new(_policy, _configuration, _clock), _monitor, NullLogger<AuthorizedToolCalls>.Instance);
    }

    async Task Because()
    {
        _firstResult = await Call();
        _root["Authorization:RevokedPrincipals"] = Principal;
        _reloadError = Catch.Exception(_root.Reload);
        _result = await Call();
        await Call();
    }

    [Fact] void should_allow_the_call_before_reload() => (_firstResult.IsError ?? false).ShouldBeFalse();
    [Fact] void should_reject_the_reload() => _reloadError.ShouldNotBeNull();
    [Fact] void should_invoke_only_the_call_before_reload() => _services.Jobs.Received(1).StopJob(Arg.Any<StopJobRequest>());
    [Fact] void should_deny_mutations_after_reload() => _result.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString().ShouldEqual(DenialCodes.PolicyInvalid);
    [Fact] void should_return_an_error() => _result.IsError.ShouldEqual(true);
    [Fact] void should_not_disclose_the_malformed_value() => _result.StructuredContent!.Value.GetRawText().Contains(Principal, StringComparison.Ordinal).ShouldBeFalse();

    ValueTask<CallToolResult> Call()
    {
        var tool = McpServerTool.Create(_catalog.Find("stop_job").Method, (object?)null, new() { Services = _toolProvider });
        var request = new RequestContext<CallToolRequestParams>(Substitute.For<McpServer>(), new() { Method = RequestMethods.ToolsCall }, new() { Name = "stop_job", Arguments = _arguments }) { Services = _toolProvider, MatchedPrimitive = tool };

        return _calls.Filter(tool.InvokeAsync)(request, CancellationToken.None);
    }

    void Destroy() => _toolProvider.Dispose();
}
