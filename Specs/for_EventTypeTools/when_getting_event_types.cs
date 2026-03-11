// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Api;

namespace Cratis.Chronicle.Mcp.Specs.for_EventTypeTools;

[Collection(ChronicleCollection.Name)]
public class when_getting_event_types(ChronicleFixture fixture) : given.a_chronicle_fixture(fixture)
{
    IEnumerable<EventType> _result = null!;

    async Task Because() =>
        _result = await EventTypeTools.GetEventTypes(ApiClient, ChronicleFixture.SharedEventStoreName);

    [Fact] void should_return_event_types() => _result.ShouldNotBeEmpty();
    [Fact] void should_include_test_event_type() => _result.ShouldContain(_ => _.Generation == 1);
    [Fact] void should_not_contain_tombstones() => _result.All(_ => !_.Tombstone).ShouldBeTrue();
}
