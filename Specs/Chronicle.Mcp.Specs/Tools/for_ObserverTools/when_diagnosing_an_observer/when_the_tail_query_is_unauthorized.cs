// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Contracts.Sequences;
using Grpc.Core;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_tail_query_is_unauthorized : given.an_observer_query
{
    Exception _error;

    void Establish() => _sequences.TailSequenceNumber(Arg.Any<TailSequenceNumberRequest>(), Arg.Any<CallContext>()).Returns(
        new QueryResult<EventSequenceTailResponse> { IsAuthorized = false, ExceptionMessages = [PrivateDetail] });

    async Task Because()
    {
        ClearCalls();
        _error = await Catch.Exception(Diagnose);
    }

    [Fact] void should_propagate_a_sanitized_denial() => Assert.Equal(StatusCode.PermissionDenied, Assert.IsType<RpcException>(_error).StatusCode);
    [Fact] void should_not_echo_upstream_details() => Assert.DoesNotContain(PrivateDetail, _error.Message);
    [Fact] void should_not_continue_after_denial() => ShouldOnlyRead(1, 1, 1);
}
