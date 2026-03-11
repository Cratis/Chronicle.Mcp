// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Api;

namespace Cratis.Chronicle.Mcp.Specs.for_ObserverTools;

[Collection(ChronicleCollection.Name)]
public class when_getting_observers(ChronicleFixture fixture) : given.a_chronicle_fixture(fixture)
{
    IEnumerable<ObserverInformation> _result = null!;

    async Task Because() =>
        _result = await ObserverTools.GetObservers(ApiClient, ChronicleFixture.SharedEventStoreName);

    [Fact] void should_return_observers() => _result.ShouldNotBeNull();
}
