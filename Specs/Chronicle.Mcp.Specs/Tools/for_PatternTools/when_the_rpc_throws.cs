// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Patterns;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools;

public class when_the_rpc_throws : given.a_pattern_query
{
    RpcException _failure;
    Exception _error;

    void Establish()
    {
        _failure = new(new(StatusCode.PermissionDenied, "Denied"));
        _patterns.MatchingPatterns(Arg.Any<MatchingPatternsRequest>(), Arg.Any<CallContext>()).ThrowsAsync(_failure);
    }

    async Task Because() => _error = await Catch.Exception(() => PatternTools.GetPatterns(_services, _configuration, "Group", _context));

    [Fact] void should_propagate_the_rpc_failure() => _error.ShouldBeSame(_failure);
}
