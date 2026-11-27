// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;
using Grpc.Core;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_recheck_is_unavailable : given.an_observer_query
{
    void Establish() => _observers.GetObservers(Arg.Any<AllObserversRequest>(), Arg.Any<CallContext>()).Returns(
        _ => Task.FromResult<IEnumerable<ObserverInformation>>([_observer]),
        _ => Task.FromException<IEnumerable<ObserverInformation>>(new RpcException(new(StatusCode.Unavailable, PrivateDetail))));

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_partial_not_disappearance() => _result.Status.ShouldEqual("partial");
    [Fact] void should_report_unverified_freshness() => _result.Freshness.ShouldEqual("unverified");
    [Fact] void should_keep_the_first_observer_read() => _result.Observer!.RunningState.ShouldEqual("Active");
    [Fact] void should_not_retry() => ShouldOnlyRead(2, 1, 1);
}
