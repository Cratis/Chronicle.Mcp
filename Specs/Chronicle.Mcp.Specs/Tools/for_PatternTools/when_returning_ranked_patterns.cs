// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Patterns;

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools;

public class when_returning_ranked_patterns : given.a_pattern_query
{
    void Establish() => _response.Data =
    [
        new BehaviorPatternDetailsResponse { Id = "z-first", Confidence = 0.6, Weight = 0.2 },
        new BehaviorPatternDetailsResponse { Id = "a-second", Confidence = 0.9, Weight = 0.8 }
    ];

    async Task Because() => _result = await PatternTools.GetPatterns(_services, _configuration, "Group", _context);

    [Fact] void should_preserve_the_authoritative_collection() => _result.ShouldBeSame(_response.Data);
    [Fact] void should_not_rerank_by_confidence_weight_or_id() => _result.Select(pattern => pattern.Id).ShouldEqual(["z-first", "a-second"]);
}
