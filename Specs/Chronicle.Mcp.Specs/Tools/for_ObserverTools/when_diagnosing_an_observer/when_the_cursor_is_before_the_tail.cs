// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_cursor_is_before_the_tail : given.an_observer_query
{
    void Establish()
    {
        _observer.LastHandledEventSequenceNumber = 7;
        _observer.NextEventSequenceNumber = 8;
        _observer.TailEventSequenceNumber = 12;
        WithTail(100);
    }

    async Task Because() => _result = await Diagnose();

    [Fact] void should_report_before_tail() => _result.PositionRelativeToTail.ShouldEqual("before_tail");
    [Fact] void should_keep_the_independent_sequence_tail() => _result.SequenceTail.ShouldEqual(100UL);
    [Fact] void should_keep_the_observer_reported_tail() => _result.Observer!.TailEventSequenceNumber.ShouldEqual(12UL);
    [Fact] void should_not_infer_progress() => _result.Progress.ShouldEqual("unknown");
    [Fact] void should_not_report_the_distance_as_a_count() => Assert.DoesNotContain("93", JsonSerializer.Serialize(_result));
}
