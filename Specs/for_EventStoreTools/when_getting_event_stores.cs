// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Specs.for_EventStoreTools;

[Collection(ChronicleCollection.Name)]
public class when_getting_event_stores(ChronicleFixture fixture) : given.a_chronicle_fixture(fixture)
{
    IEnumerable<string> _result = null!;

    async Task Because() =>
        _result = await EventStoreTools.GetEventStores(ApiClient);

    [Fact] void should_return_event_stores() => _result.ShouldNotBeEmpty();
    [Fact] void should_contain_seeded_event_store() => _result.ShouldContain(ChronicleFixture.SharedEventStoreName);
}
