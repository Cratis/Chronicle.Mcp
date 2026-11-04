// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools;

public class when_querying_with_negative_values : given.a_pattern_query
{
    async Task Because() => _result = await PatternTools.GetPatterns(
        _services, _configuration, "Group", _context, minimumConfidence: -0.5, maximumResults: -7);

    [Fact] void should_forward_negative_confidence_for_backend_default_resolution() => _request.MinimumConfidence.ShouldEqual(-0.5);
    [Fact] void should_forward_negative_count_for_backend_default_resolution() => _request.MaximumResults.ShouldEqual(-7);
}
