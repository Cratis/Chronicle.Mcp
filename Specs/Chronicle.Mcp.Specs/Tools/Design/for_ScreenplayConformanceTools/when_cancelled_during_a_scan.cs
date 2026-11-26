// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Sequences;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_cancelled_during_a_scan : a_namespace_audit
{
    void Establish()
    {
        _tail = 500;
        Argument("toSequence", 500);
    }

    async Task Because()
    {
        using var cancellation = new CancellationTokenSource();
        _sequences.MetadataAt(Arg.Any<MetadataAtRequest>(), Arg.Any<CallContext>()).Returns(_ =>
        {
            cancellation.Cancel();
            return Success<IEnumerable<EventMetadataResponse>>(_metadata);
        });
        await Audit(cancellation.Token);
    }

    [Fact]
    void should_return_a_cancellation_error() => Body.GetProperty("error").GetProperty("code").GetString().ShouldEqual("conformance_cancelled");

    [Fact]
    void should_not_request_another_page() => _ = _sequences.Received(1).MetadataAt(Arg.Any<MetadataAtRequest>(), Arg.Any<CallContext>());
}
