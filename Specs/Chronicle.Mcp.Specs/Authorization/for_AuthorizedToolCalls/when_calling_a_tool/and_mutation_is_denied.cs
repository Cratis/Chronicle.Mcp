// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Jobs;
using ModelContextProtocol.Protocol;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_mutation_is_denied : given.a_filter
{
    void Establish() => _grant.EventStore = "somewhere-else";
    async Task Because() => _result = await Call("delete_job");
    [Fact] void should_not_reach_the_job_tool() => _services.Jobs.DidNotReceive().DeleteJob(Arg.Any<DeleteJobRequest>());
    [Fact] void should_return_an_error() => _result.IsError.ShouldEqual(true);
    [Fact] void should_return_the_stable_envelope() => _result.StructuredContent!.Value.GetRawText().ShouldEqual(/*lang=json,strict*/ "{\"error\":{\"code\":\"not_authorized\",\"operation\":\"delete_job\"}}");
    [Fact] void should_return_the_same_text_json() => ((TextContentBlock)_result.Content.Single()).Text.ShouldEqual(_result.StructuredContent!.Value.GetRawText());
    [Fact] void should_not_expose_store() => _result.StructuredContent!.Value.GetRawText().Contains(Store, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_not_expose_namespace() => _result.StructuredContent!.Value.GetRawText().Contains(Namespace, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_not_expose_target() => _result.StructuredContent!.Value.GetRawText().Contains(Target, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_not_expose_principal() => _result.StructuredContent!.Value.GetRawText().Contains(Principal, StringComparison.Ordinal).ShouldBeFalse();
}
