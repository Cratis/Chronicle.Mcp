// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_missing_in_the_selected_scope : given.an_observer_query
{
    void Establish() => _observers.GetObservers(Arg.Any<AllObserversRequest>(), Arg.Any<CallContext>()).Returns(call =>
    {
        var request = call.Arg<AllObserversRequest>();
        return string.Equals(request.EventStore, "OtherStore", StringComparison.Ordinal) &&
               string.Equals(request.Namespace, "OtherTenant", StringComparison.Ordinal) ? [_observer] : [];
    });

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_not_found() => _result.Status.ShouldEqual("not_found");
    [Fact] void should_not_return_the_other_scope() => _result.Observer.ShouldBeNull();
    [Fact] void should_not_read_enrichment_or_search_other_scopes() => ShouldOnlyRead(1, 0, 0);
}
