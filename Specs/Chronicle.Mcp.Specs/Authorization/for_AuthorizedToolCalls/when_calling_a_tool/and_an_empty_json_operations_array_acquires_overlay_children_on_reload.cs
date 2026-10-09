// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Jobs;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_an_empty_json_operations_array_acquires_overlay_children_on_reload : given.a_layered_filter
{
    async Task Because()
    {
        _firstResult = await Call();
        WithEmptyArray("Authorization:Grants:0:Operations");
        _overlay.Replace(new Dictionary<string, string?> { ["Authorization:Grants:0:Operations:0"] = "stop_job" });
        _reloadError = Catch.Exception(_jsonProvider.Publish);
        _result = await Call();
        await Call();
    }

    [Fact] void should_allow_the_call_before_reload() => (_firstResult.IsError ?? false).ShouldBeFalse();
    [Fact] void should_reject_the_reload() => _reloadError.ShouldNotBeNull();
    [Fact] void should_invoke_only_the_call_before_reload() => _services.Jobs.Received(1).StopJob(Arg.Any<StopJobRequest>());
    [Fact] void should_deny_subsequent_mutations() => _result.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString().ShouldEqual(DenialCodes.PolicyInvalid);
    [Fact] void should_return_an_error() => _result.IsError.ShouldEqual(true);
}
