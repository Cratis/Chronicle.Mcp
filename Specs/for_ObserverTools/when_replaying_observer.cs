// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Api;

namespace Cratis.Chronicle.Mcp.Specs.for_ObserverTools;

[Collection(ChronicleCollection.Name)]
public class when_replaying_observer(ChronicleFixture fixture) : given.a_chronicle_fixture(fixture)
{
    string _result = null!;
    string _observerId = null!;

    async Task Establish()
    {
        var observers = await ObserverTools.GetObservers(ApiClient, ChronicleFixture.SharedEventStoreName);
        var observer = observers.FirstOrDefault();
        _observerId = observer?.Id ?? "unknown";
    }

    async Task Because() =>
        _result = await ObserverTools.ReplayObserver(ApiClient, ChronicleFixture.SharedEventStoreName, "Default", _observerId);

    [Fact] void should_return_confirmation_message() => _result.ShouldContain("replay initiated");
    [Fact] void should_include_observer_id() => _result.ShouldContain(_observerId);
}
