// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_active_at_position_zero : given.an_observer_query
{
    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_an_observed_snapshot() => _result.Status.ShouldEqual("observed");
    [Fact] void should_confirm_the_observer_facts_were_unchanged() => _result.Freshness.ShouldEqual("unchanged");
    [Fact] void should_report_active() => _result.Observer!.RunningState.ShouldEqual("Active");
    [Fact] void should_report_the_state_change_time() => _result.Observer!.RunningStateChangedAt.ShouldEqual(Occurred);
    [Fact] void should_report_next_position_one() => _result.Observer!.NextEventSequenceNumber.ShouldEqual(1UL);
    [Fact] void should_keep_zero_as_a_real_handled_position() => _result.Observer!.LastHandledEventSequenceNumber.ShouldEqual(0UL);
    [Fact] void should_read_zero_as_a_real_tail() => _result.SequenceTail.ShouldEqual(0UL);
    [Fact] void should_report_at_tail() => _result.PositionRelativeToTail.ShouldEqual("at_tail");
    [Fact] void should_keep_the_reported_count() => _result.Observer!.HandledEventCount.ShouldEqual(42UL);
    [Fact] void should_not_infer_progress() => _result.Progress.ShouldEqual("unknown");
    [Fact] void should_explain_why_progress_is_unknown() => _result.ProgressReason.ShouldEqual("sampled_progress_not_available");
    [Fact] void should_only_use_the_allowed_reads() => ShouldOnlyRead(2, 1, 1);
    [Fact] void should_use_the_configured_scope_for_all_reads() => ShouldReadScope();
}
