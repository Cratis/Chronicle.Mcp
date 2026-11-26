// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Contracts.Sequences;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_tail_query_fails : given.an_observer_query
{
    void Establish()
    {
        WithFailures(Failure("sample-partition"));
        _sequences.TailSequenceNumber(Arg.Any<TailSequenceNumberRequest>(), Arg.Any<CallContext>()).Returns(
            new QueryResult<EventSequenceTailResponse> { ExceptionMessages = [PrivateDetail], Data = new() { SequenceNumber = 0 } });
    }

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_partial() => _result.Status.ShouldEqual("partial");
    [Fact] void should_report_the_query_failure() => _result.TailReason.ShouldEqual("query_failed");
    [Fact] void should_not_infer_a_tail_from_default_data() => _result.SequenceTail.ShouldBeNull();
    [Fact] void should_keep_the_other_evidence() => Assert.Single(_result.Failures.Rows);
    [Fact] void should_not_echo_raw_details() => Assert.DoesNotContain(PrivateDetail, JsonSerializer.Serialize(_result));
    [Fact] void should_only_use_the_allowed_reads() => ShouldOnlyRead(2, 1, 1);
}
