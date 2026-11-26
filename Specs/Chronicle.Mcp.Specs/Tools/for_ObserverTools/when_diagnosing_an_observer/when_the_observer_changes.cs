// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_observer_changes : given.an_observer_query
{
    void Establish()
    {
        var final = ActiveObserver();
        final.RunningState = ObserverRunningState.Replaying;
        final.LastHandledEventSequenceNumber = 9;
        _observers.GetObservers(Arg.Any<AllObserversRequest>(), Arg.Any<CallContext>()).Returns(new[] { _observer }.AsEnumerable(), [final]);
        WithFailures(Failure("old-failure"));
    }

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_the_change() => _result.Status.ShouldEqual("changed");
    [Fact] void should_use_final_observer_facts() => _result.Observer!.LastHandledEventSequenceNumber.ShouldEqual(9UL);
    [Fact] void should_discard_old_failure_associations() => Assert.Empty(_result.Failures.Rows);
    [Fact] void should_discard_the_intermediate_tail() => _result.SequenceTail.ShouldBeNull();
    [Fact] void should_not_retry() => ShouldOnlyRead(2, 1, 1);
}
