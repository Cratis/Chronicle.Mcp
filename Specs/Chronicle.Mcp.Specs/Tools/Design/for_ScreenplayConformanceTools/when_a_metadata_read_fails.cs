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
    [InlineData(null, "chronicle_denied")]
    public async Task should_sanitize_a_failed_read_on_the_second_page(StatusCode? status, string errorCode)
    {
        var failure = status is { } transportStatus
            ? Task.FromException<QueryResult<IEnumerable<EventMetadataResponse>>>(new RpcException(new Status(transportStatus, "private-transport-details")))
            : Task.FromResult(new QueryResult<IEnumerable<EventMetadataResponse>> { IsAuthorized = false, ExceptionMessages = ["private-query-exception"] });
        _sequences.MetadataAt(Arg.Is<MetadataAtRequest>(request => request.SequenceNumbers.First() == 500), Arg.Any<CallContext>()).Returns(failure);
        await Audit(TestContext.Current.CancellationToken);
        VerifyError(errorCode);
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
