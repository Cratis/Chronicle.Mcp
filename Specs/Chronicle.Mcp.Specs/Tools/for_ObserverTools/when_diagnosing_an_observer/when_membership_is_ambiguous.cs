// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_membership_is_ambiguous : given.an_observer_query
{
    void Establish() => _observers.GetObservers(Arg.Any<AllObserversRequest>(), Arg.Any<CallContext>()).Returns(new[] { _observer, ActiveObserver() }.AsEnumerable());

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_not_treat_ambiguity_as_absence() => _result.Status.ShouldEqual("ambiguous_observer");
    [Fact] void should_not_pick_a_row() => _result.Observer.ShouldBeNull();
    [Fact] void should_not_enrich_an_ambiguous_target() => ShouldOnlyRead(1, 0, 0);
}
