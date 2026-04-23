// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Api;

namespace Cratis.Chronicle.Mcp.Specs.for_RecommendationTools;

[Collection(ChronicleCollection.Name)]
public class when_getting_recommendations(ChronicleFixture fixture) : given.a_chronicle_fixture(fixture)
{
    IEnumerable<Recommendation> _result = null!;

    async Task Because() =>
        _result = await RecommendationTools.GetRecommendations(ApiClient, ChronicleFixture.SharedEventStoreName);

    [Fact] void should_return_recommendations() => _result.ShouldNotBeNull();
}
