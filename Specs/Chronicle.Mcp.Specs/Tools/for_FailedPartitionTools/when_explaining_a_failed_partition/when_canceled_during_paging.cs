// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Contracts.Sequences;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class when_canceled_during_paging : given.a_failed_partition_tool_call
{
    Exception _error;

    void Establish() => _sequences.Page(Arg.Any<PageRequest>(), Arg.Any<CallContext>()).Returns(call =>
    {
        _callContexts.Add(call.Arg<CallContext>());
        _cancellation.Cancel();
        return _page;
    });

    async Task Because() => _error = await Catch.Exception(() => Invoke("explain_failed_partition", new Dictionary<string, JsonElement>
    {
        ["failedPartitionId"] = JsonSerializer.SerializeToElement(FailureId)
    }));

    [Fact] void should_pass_the_mcp_token_to_all_reads() => _callContexts.Select(context => context.CancellationToken).ShouldEqual(Enumerable.Repeat(_cancellation.Token, 3));
    [Fact] void should_not_recheck_after_cancellation() => _failureRequests.Count.ShouldEqual(1);
    [Fact] void should_propagate_the_cancellation() => Assert.IsType<OperationCanceledException>(_error, exactMatch: false);
}
