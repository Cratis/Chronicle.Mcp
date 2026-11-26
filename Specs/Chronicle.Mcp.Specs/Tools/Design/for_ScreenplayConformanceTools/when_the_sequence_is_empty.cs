// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_the_sequence_is_empty : a_namespace_audit
{
    void Establish() => _tail = ulong.MaxValue - 2;

    Task Because() => Audit();

    [Fact]
    void should_report_complete_empty_evidence()
    {
        var sequence = Body.GetProperty("coverage").GetProperty("sequence");
        sequence.GetProperty("complete").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    void should_not_request_any_positions() => _pages.ShouldBeEmpty();
}
