// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Api;

namespace Cratis.Chronicle.Mcp.Specs.for_ObserverTools;

[Collection(ChronicleCollection.Name)]
public class when_getting_observers_for_namespace(ChronicleFixture fixture) : given.a_chronicle_fixture(fixture)
{
    const string EventStoreName = "observer-ns-tools-spec";

    IEnumerable<ObserverInformation> _result = null!;

    async Task Establish()
    {
        var eventStore = await Client.GetEventStore(EventStoreName);
        await eventStore.EventLog.Append("test-source", new TestEvent("seeded content"));
    }

    async Task Because() =>
        _result = await ObserverTools.GetObserversForNamespace(ApiClient, EventStoreName, "Default");

    [Fact] void should_return_observers() => _result.ShouldNotBeNull();
}
