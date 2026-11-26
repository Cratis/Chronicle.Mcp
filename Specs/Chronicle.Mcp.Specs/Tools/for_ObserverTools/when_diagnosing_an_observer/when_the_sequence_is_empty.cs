// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_sequence_is_empty : given.an_observer_query
{
    void Establish()
    {
        _observer.LastHandledEventSequenceNumber = ulong.MaxValue;
        _observer.NextEventSequenceNumber = ulong.MaxValue - 1;
        _observer.TailEventSequenceNumber = ulong.MaxValue - 2;
        WithTail(ulong.MaxValue - 2);
    }

    async Task Because() => _result = await Diagnose();

    [Fact] void should_distinguish_no_tail_from_a_failed_read() => _result.TailStatus.ShouldEqual("available");
    [Fact] void should_report_no_tail() => _result.TailReason.ShouldEqual("no_tail");
    [Fact] void should_not_report_a_tail_position() => _result.SequenceTail.ShouldBeNull();
    [Fact] void should_not_report_a_sentinel_next_position() => _result.Observer!.NextEventSequenceNumber.ShouldBeNull();
    [Fact] void should_not_report_a_sentinel_handled_position() => _result.Observer!.LastHandledEventSequenceNumber.ShouldBeNull();
    [Fact] void should_not_report_a_sentinel_observer_tail() => _result.Observer!.TailEventSequenceNumber.ShouldBeNull();
    [Fact] void should_not_compare_positions() => _result.PositionRelativeToTail.ShouldEqual("unknown");
}
