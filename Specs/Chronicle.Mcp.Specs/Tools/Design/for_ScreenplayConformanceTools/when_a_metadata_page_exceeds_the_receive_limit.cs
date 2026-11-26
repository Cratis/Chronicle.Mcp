// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Contracts.Sequences;
using Grpc.Core;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_a_metadata_page_exceeds_the_receive_limit : a_namespace_audit
{
    void Establish()
    {
        _tail = 999;
        Argument("toSequence", 999);
        _metadata.Clear();
        _metadata.AddRange(Enumerable.Range(0, 1000).Select(index => Append((ulong)index, "Registered", "Register")));
        _sequences.MetadataAt(Arg.Is<MetadataAtRequest>(request => request.SequenceNumbers.Count() > 250), Arg.Any<CallContext>())
            .Returns(Task.FromException<QueryResult<IEnumerable<EventMetadataResponse>>>(new RpcException(new Status(StatusCode.ResourceExhausted, "response too large"))));
    }

    Task Because() => Audit();

    [Fact]
    void should_retry_from_the_same_position_and_keep_the_reduced_page_size() =>
        _sequences.ReceivedCalls().Where(call => call.GetMethodInfo().Name == nameof(IEventSequences.MetadataAt))
            .Select(call => (MetadataAtRequest)call.GetArguments()[0]!)
            .Select(request => (request.SequenceNumbers.First(), request.SequenceNumbers.Count()))
            .ShouldEqual((0ul, 500), (0ul, 250), (250ul, 250), (500ul, 250), (750ul, 250));

    [Fact]
    void should_count_every_append_once_and_preserve_complete_coverage()
    {
        var coverage = Body.GetProperty("coverage");
        coverage.GetProperty("appendedEvents").GetInt64().ShouldEqual(1000L);
        coverage.GetProperty("sequence").GetProperty("complete").GetBoolean().ShouldBeTrue();
    }
}
