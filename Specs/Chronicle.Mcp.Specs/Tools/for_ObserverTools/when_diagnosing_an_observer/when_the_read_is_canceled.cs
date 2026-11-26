// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Sequences;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_read_is_canceled : given.an_observer_query
{
    readonly RpcException _cancellation = new(new(StatusCode.Cancelled, "Synthetic cancellation"));
    Exception _error;

    void Establish() => _sequences.TailSequenceNumber(Arg.Any<TailSequenceNumberRequest>(), Arg.Any<CallContext>()).ThrowsAsync(_cancellation);

    async Task Because()
    {
        ClearCalls();
        _error = await Catch.Exception(Diagnose);
    }

    [Fact] void should_propagate_cancellation() => _error.ShouldBeSame(_cancellation);
    [Fact] void should_not_continue_after_cancellation() => ShouldOnlyRead(1, 1, 1);
}
