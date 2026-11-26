// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_sequence_does_not_match : given.an_observer_query
{
    void Establish() => _observer.EventSequenceId = "Event-log";

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_a_mismatch() => _result.Status.ShouldEqual("sequence_mismatch");
    [Fact] void should_echo_the_selected_sequence() => _result.EventSequenceId.ShouldEqual(Sequence);
    [Fact] void should_report_the_observed_sequence() => _result.Observer!.EventSequenceId.ShouldEqual("Event-log");
    [Fact] void should_not_enrich_the_wrong_sequence() => ShouldOnlyRead(1, 0, 0);
}
