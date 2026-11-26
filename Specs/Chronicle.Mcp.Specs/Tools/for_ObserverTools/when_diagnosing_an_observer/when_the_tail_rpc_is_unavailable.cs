// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Sequences;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_tail_rpc_is_unavailable : given.an_observer_query
{
    void Establish() => _sequences.TailSequenceNumber(Arg.Any<TailSequenceNumberRequest>(), Arg.Any<CallContext>())
        .ThrowsAsync(new RpcException(new(StatusCode.Unavailable, PrivateDetail)));

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_partial() => _result.Status.ShouldEqual("partial");
    [Fact] void should_report_an_unavailable_tail() => _result.TailStatus.ShouldEqual("unavailable");
    [Fact] void should_use_a_fixed_reason() => _result.TailReason.ShouldEqual("rpc_failed");
    [Fact] void should_distinguish_transport_failure_from_an_empty_sequence() => _result.SequenceTail.ShouldBeNull();
    [Fact] void should_preserve_failure_evidence() => _result.Failures.Status.ShouldEqual("available");
    [Fact] void should_not_retry_or_fetch_events() => ShouldOnlyRead(2, 1, 1);
}
