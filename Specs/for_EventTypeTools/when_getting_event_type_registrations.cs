// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Api;

namespace Cratis.Chronicle.Mcp.Specs.for_EventTypeTools;

[Collection(ChronicleCollection.Name)]
public class when_getting_event_type_registrations(ChronicleFixture fixture) : given.a_chronicle_fixture(fixture)
{
    IEnumerable<EventTypeRegistration> _result = null!;

    async Task Because() =>
        _result = await EventTypeTools.GetEventTypeRegistrations(ApiClient, ChronicleFixture.SharedEventStoreName);

    [Fact] void should_return_registrations() => _result.ShouldNotBeEmpty();
    [Fact] void should_include_schema() => _result.First().Schema.ShouldNotBeEmpty();
    [Fact] void should_have_valid_owner_name() => _result.First().OwnerName.ShouldNotBeEmpty();
    [Fact] void should_have_valid_source_name() => _result.First().SourceName.ShouldNotBeEmpty();
}
