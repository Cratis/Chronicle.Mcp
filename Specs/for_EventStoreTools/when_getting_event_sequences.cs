// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Specs.for_EventStoreTools;

[Collection(ChronicleCollection.Name)]
public class when_getting_event_sequences(ChronicleFixture fixture) : given.a_chronicle_fixture(fixture)
{
    IEnumerable<string> _result = null!;

    async Task Because() =>
        _result = await EventStoreTools.GetEventSequences(ApiClient, ChronicleFixture.SharedEventStoreName);

    [Fact] void should_return_event_sequences() => _result.ShouldNotBeEmpty();
    [Fact] void should_contain_event_log() => _result.ShouldContain("event-log");
}
