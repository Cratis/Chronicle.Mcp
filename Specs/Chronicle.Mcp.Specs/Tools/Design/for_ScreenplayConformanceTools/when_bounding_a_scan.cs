// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_bounding_a_scan : a_namespace_audit
{
    void Establish()
    {
        _tail = 3000;
        _metadata.Clear();
        _metadata.AddRange([Append(0, "Registered"), Append(1, "Registered"), Append(2, "Registered"), Append(3001, "Registered")]);
        _metadata[1].Occurred = Now.AddMinutes(5);
        Argument("toSequence", long.MaxValue);
        Argument("occurredFrom", Now);
        Argument("occurredTo", Now.AddMinutes(3));
    }

    Task Because() => Audit();

    [Fact]
    void should_mark_filtered_and_missing_positions_even_when_metadata_is_sparse_and_time_is_unordered()
    {
        var coverage = Body.GetProperty("coverage");
        var sequence = coverage.GetProperty("sequence");
        coverage.GetProperty("appendedEvents").GetInt64().ShouldEqual(2L);
        sequence.GetProperty("capturedTail").GetInt64().ShouldEqual(3000L);
        sequence.GetProperty("lastExamined").GetInt64().ShouldEqual(1999L);
        sequence.GetProperty("complete").GetBoolean().ShouldBeFalse();
        sequence.GetProperty("unexamined").EnumerateArray().Select(gap => (gap.GetProperty("from").GetInt64(), gap.GetProperty("to").GetInt64()))
            .ShouldEqual((1L, 1L), (3L, 1999L));
    }

    [Fact]
    void should_contain_a_capped_continuation_independent_of_report_truncation()
    {
        _pages.Select(page => page.SequenceNumbers.Count()).ShouldEqual(500, 500, 500, 500);
        Body.GetProperty("scanCapped").GetBoolean().ShouldBeTrue();
        Body.GetProperty("reportTruncated").GetBoolean().ShouldBeFalse();
        var continuation = Body.GetProperty("continuation");
        continuation.GetProperty("fromSequence").GetInt64().ShouldEqual(2000L);
        continuation.GetProperty("toSequence").GetInt64().ShouldEqual(3000L);
    }
}
