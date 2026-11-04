// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools;

public class when_confidence_exceeds_the_documented_domain : given.a_pattern_query
{
    async Task Because() => _result = await PatternTools.GetPatterns(
        _services, _configuration, "Group", _context, minimumConfidence: 1.5);

    [Fact] void should_forward_confidence_without_local_range_rejection() => _request.MinimumConfidence.ShouldEqual(1.5);
}
