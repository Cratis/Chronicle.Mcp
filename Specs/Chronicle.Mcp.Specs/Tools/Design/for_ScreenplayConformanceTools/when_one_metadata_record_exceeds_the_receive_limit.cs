// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Contracts.Sequences;
using Grpc.Core;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_one_metadata_record_exceeds_the_receive_limit : a_namespace_audit
{
    void Establish()
    {
        _sequences.MetadataAt(Arg.Any<MetadataAtRequest>(), Arg.Any<CallContext>())
            .Returns(Task.FromException<QueryResult<IEnumerable<EventMetadataResponse>>>(new RpcException(new Status(StatusCode.ResourceExhausted, "response too large"))));
    }

    Task Because() => Audit();

    [Fact]
    void should_fail_closed_without_partial_evidence()
    {
        Body.GetProperty("error").GetProperty("code").GetString().ShouldEqual("conformance_read_limit");
        Body.TryGetProperty("coverage", out _).ShouldBeFalse();
    }

    [Fact]
    void should_stop_after_retrying_the_smallest_page() =>
        _sequences.ReceivedCalls().Where(call => call.GetMethodInfo().Name == nameof(IEventSequences.MetadataAt))
            .Select(call => ((MetadataAtRequest)call.GetArguments()[0]!).SequenceNumbers.Count())
            .ShouldEqual(3, 1);
}
