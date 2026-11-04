// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools;

public class when_querying_with_cancellation : given.a_pattern_query
{
    CancellationTokenSource _cancellation;

    void Establish() => _cancellation = new();

    async Task Because() => _result = await PatternTools.GetPatterns(
        _services, _configuration, "Group", _context, cancellationToken: _cancellation.Token);

    [Fact] void should_forward_the_cancellation_token() => _callContext.CancellationToken.ShouldEqual(_cancellation.Token);

    void Destroy() => _cancellation.Dispose();
}
