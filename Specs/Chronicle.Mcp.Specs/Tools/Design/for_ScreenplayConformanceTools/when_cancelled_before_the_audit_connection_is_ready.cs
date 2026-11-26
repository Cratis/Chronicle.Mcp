// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_cancelled_before_the_audit_connection_is_ready : a_namespace_audit
{
    async Task Because()
    {
        using var cancellation = new CancellationTokenSource();
        _auditServices.BeforeRead = _ => cancellation.CancelAsync();
        await Audit(cancellation.Token);
    }

    [Fact]
    void should_return_a_cancellation_error() => Body.GetProperty("error").GetProperty("code").GetString().ShouldEqual("conformance_cancelled");

    [Fact]
    void should_not_read_chronicle() => _reads.ReceivedCalls().ShouldBeEmpty();
}
