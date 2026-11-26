// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Contracts.Observation;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_failure_lookup_is_unavailable : given.an_observer_query
{
    void Establish() => _failures.GetFailedPartitions(Arg.Any<GetFailedPartitionsRequest>(), Arg.Any<CallContext>())
        .ThrowsAsync(new RpcException(new(StatusCode.Unavailable, PrivateDetail)));

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_partial() => _result.Status.ShouldEqual("partial");
    [Fact] void should_not_report_a_successful_empty_lookup() => _result.Failures.Status.ShouldEqual("unavailable");
    [Fact] void should_preserve_the_tail_evidence() => _result.SequenceTail.ShouldEqual(0UL);
    [Fact] void should_not_echo_raw_details() => Assert.DoesNotContain(PrivateDetail, JsonSerializer.Serialize(_result));
    [Fact] void should_only_use_the_allowed_reads() => ShouldOnlyRead(2, 1, 1);
}
