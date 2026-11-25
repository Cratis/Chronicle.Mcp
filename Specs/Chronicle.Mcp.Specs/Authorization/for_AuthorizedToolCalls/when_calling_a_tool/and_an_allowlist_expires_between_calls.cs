// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Jobs;
using ModelContextProtocol.Protocol;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_an_allowlist_expires_between_calls : given.a_filter
{
    CallToolResult _firstResult;

    void Establish() => _settings.Authorization = new() { ExpiresAt = Now.AddMinutes(1), Allowlist = [new() { EventStore = Store, Namespace = Namespace, Operations = ["stop_job"] }] };

    async Task Because()
    {
        _firstResult = await Call("stop_job");
        _clock.GetUtcNow().Returns(Now.AddMinutes(1));
        _result = await Call("stop_job");
    }

    [Fact] void should_forward_the_initial_request() => (_firstResult.IsError ?? false).ShouldBeFalse();
    [Fact] void should_not_invoke_after_expiration() => _services.Jobs.Received(1).StopJob(Arg.Any<StopJobRequest>());
    [Fact] void should_return_the_stable_expiration_code() => _result.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString().ShouldEqual(DenialCodes.PolicyExpired);
}
