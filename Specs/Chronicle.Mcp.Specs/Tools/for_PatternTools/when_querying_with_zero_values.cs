// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools;

public class when_querying_with_zero_values : given.a_pattern_query
{
    async Task Because() => _result = await PatternTools.GetPatterns(
        _services, _configuration, "Group", _context, minimumConfidence: 0, maximumResults: 0);

    [Fact] void should_forward_zero_confidence() => _request.MinimumConfidence.ShouldEqual(0d);
    [Fact] void should_forward_zero_count() => _request.MaximumResults.ShouldEqual(0);
}
