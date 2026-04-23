// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Api;

namespace Cratis.Chronicle.Mcp.Specs.for_EventSequenceTools;

[Collection(ChronicleCollection.Name)]
public class when_getting_events(ChronicleFixture fixture) : given.a_chronicle_fixture(fixture)
{
    IEnumerable<AppendedEvent> _result = null!;

    async Task Because() =>
        _result = await EventSequenceTools.GetEvents(ApiClient, ChronicleFixture.SharedEventStoreName);

    [Fact] void should_return_events() => _result.ShouldNotBeEmpty();
    [Fact] void should_have_event_content() => _result.First().Content.ShouldNotBeEmpty();
    [Fact] void should_have_event_context() => _result.First().Context.ShouldNotBeNull();
}
