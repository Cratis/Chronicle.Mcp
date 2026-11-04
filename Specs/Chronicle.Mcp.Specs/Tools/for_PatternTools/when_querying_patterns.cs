// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Patterns;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools;

public class when_querying_patterns : given.a_pattern_query
{
    async Task Because() => _result = await PatternTools.GetPatterns(_services, _configuration, "Order/Placed", _context);

    [Fact] void should_use_the_configured_event_store() => _request.EventStore.ShouldEqual("ConfiguredStore");
    [Fact] void should_use_the_configured_namespace() => _request.Namespace.ShouldEqual("ConfiguredTenant");
    [Fact] void should_preserve_the_grouping_key() => _request.GroupingKey.ShouldEqual("Order/Placed");
    [Fact] void should_forward_context_without_filtering_or_changing_case() => _request.Context.ShouldBeSame(_context);
    [Fact] void should_leave_confidence_default_resolution_to_the_server() => _request.MinimumConfidence.ShouldEqual(0d);
    [Fact] void should_leave_count_default_resolution_to_the_server() => _request.MaximumResults.ShouldEqual(0);
    [Fact] void should_preserve_a_genuine_empty_success() => _result.ShouldBeEmpty();
    [Fact] void should_only_make_one_rpc() => _patterns.ReceivedCalls().Count().ShouldEqual(1);
    [Fact] async Task should_query_matching_patterns() => await _patterns.Received(1).MatchingPatterns(Arg.Any<MatchingPatternsRequest>(), Arg.Any<CallContext>());
}
