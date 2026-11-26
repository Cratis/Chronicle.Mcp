// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_observer_disappears : given.an_observer_query
{
    void Establish()
    {
        _observers.GetObservers(Arg.Any<AllObserversRequest>(), Arg.Any<CallContext>()).Returns(new[] { _observer }.AsEnumerable(), Enumerable.Empty<ObserverInformation>());
        WithFailures(Failure("old-failure"));
    }

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_disappearance_not_initial_absence() => _result.Status.ShouldEqual("disappeared");
    [Fact] void should_not_present_the_old_observer() => _result.Observer.ShouldBeNull();
    [Fact] void should_not_present_the_old_associations() => Assert.Empty(_result.Failures.Rows);
    [Fact] void should_not_retry() => ShouldOnlyRead(2, 1, 1);
}
