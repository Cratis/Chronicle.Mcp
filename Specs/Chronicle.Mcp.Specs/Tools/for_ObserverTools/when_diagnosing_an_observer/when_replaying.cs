// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_replaying : given.an_observer_query
{
    void Establish()
    {
        _observer.RunningState = ObserverRunningState.Replaying;
        _observer.IsReplayable = false;
        WithFailures(Failure("sample-partition"));
    }

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_replaying_regardless_of_position() => _result.Observer!.RunningState.ShouldEqual("Replaying");
    [Fact] void should_not_infer_replayability_from_state() => _result.Observer!.IsReplayable.ShouldBeFalse();
    [Fact] void should_only_use_the_allowed_reads() => ShouldOnlyRead(2, 1, 1);
}
