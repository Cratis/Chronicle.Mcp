// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_observer_changes_sequence : given.an_observer_query
{
    void Establish()
    {
        var final = ActiveObserver();
        final.EventSequenceId = "orders";
        _observers.GetObservers(Arg.Any<AllObserversRequest>(), Arg.Any<CallContext>()).Returns(new[] { _observer }.AsEnumerable(), [final]);
        WithFailures(Failure("old-failure"));
    }

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_changed() => _result.Status.ShouldEqual("changed");
    [Fact] void should_keep_the_selected_sequence() => _result.EventSequenceId.ShouldEqual(Sequence);
    [Fact] void should_report_the_final_sequence() => _result.Observer!.EventSequenceId.ShouldEqual("orders");
    [Fact] void should_discard_old_sequence_associations() => Assert.Empty(_result.Failures.Rows);
    [Fact] void should_not_switch_sequences_or_retry() => ShouldReadScope();
    [Fact] void should_only_use_the_allowed_reads() => ShouldOnlyRead(2, 1, 1);
}
