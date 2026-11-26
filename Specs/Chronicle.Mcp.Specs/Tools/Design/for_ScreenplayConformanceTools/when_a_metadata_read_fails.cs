// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Contracts.Sequences;
using Grpc.Core;
using ModelContextProtocol;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_a_metadata_read_fails : a_namespace_audit
{
    void Establish()
    {
        _tail = 500;
        Argument("toSequence", 500);
    }

    [Theory]
    [InlineData(StatusCode.PermissionDenied, "chronicle_denied")]
    [InlineData(StatusCode.ResourceExhausted, "conformance_read_limit")]
    [InlineData(StatusCode.DeadlineExceeded, "conformance_timeout")]
    public async Task should_sanitize_a_transport_failure_on_the_second_page(StatusCode status, string errorCode)
    {
        _sequences.MetadataAt(Arg.Is<MetadataAtRequest>(request => request.SequenceNumbers.First() == 500), Arg.Any<CallContext>())
            .Returns(Task.FromException<QueryResult<IEnumerable<EventMetadataResponse>>>(new RpcException(new Status(status, "private-transport-details"))));
        await Audit();
        VerifyError(errorCode);
    }

    [Fact]
    public async Task should_sanitize_a_query_refusal_on_the_second_page()
    {
        _sequences.MetadataAt(Arg.Is<MetadataAtRequest>(request => request.SequenceNumbers.First() == 500), Arg.Any<CallContext>())
            .Returns(new QueryResult<IEnumerable<EventMetadataResponse>> { IsAuthorized = false, ExceptionMessages = ["private-query-exception"] });
        await Audit();
        VerifyError("chronicle_denied");
    }

    [Fact]
    public async Task should_not_start_another_page_after_caller_cancellation()
    {
        using var cancellation = new CancellationTokenSource();
        _sequences.MetadataAt(Arg.Any<MetadataAtRequest>(), Arg.Any<CallContext>()).Returns(_ =>
        {
            cancellation.Cancel();
            return Success<IEnumerable<EventMetadataResponse>>(_metadata);
        });
        await Audit(cancellation.Token);
        VerifyError("conformance_cancelled");
        _ = _sequences.Received(1).MetadataAt(Arg.Any<MetadataAtRequest>(), Arg.Any<CallContext>());
    }

    void VerifyError(string code)
    {
        var content = _result.StructuredContent!.Value;
        if (content.TryGetProperty("untrustedData", out var envelope)) content = envelope.GetProperty("content");
        content.GetProperty("error").GetProperty("code").GetString().ShouldEqual(code);
        content.TryGetProperty("coverage", out _).ShouldBeFalse();
        JsonSerializer.Serialize(_result, McpJsonUtilities.ProtocolOptions).Contains("private-", StringComparison.Ordinal).ShouldBeFalse();
    }
}
