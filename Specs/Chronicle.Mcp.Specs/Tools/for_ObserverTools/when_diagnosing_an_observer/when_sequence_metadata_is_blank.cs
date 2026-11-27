// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_sequence_metadata_is_blank : given.an_observer_query
{
    void Establish() => _observer.EventSequenceId = " ";

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_partial() => _result.Status.ShouldEqual("partial");
    [Fact] void should_explain_why_metadata_is_incomplete() => _result.ObserverReason.ShouldEqual("observer_sequence_missing");
    [Fact] void should_not_enrich_unverified_metadata() => ShouldOnlyRead(1, 0, 0);
}
