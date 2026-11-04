// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools;

public class when_querying_with_explicit_values : given.a_pattern_query
{
    async Task Because() => _result = await PatternTools.GetPatterns(
        _services, _configuration, "Explicit/Group", _context, "ExplicitStore", "ExplicitTenant", 0.85, 100000);

    [Fact] void should_preserve_the_explicit_event_store() => _request.EventStore.ShouldEqual("ExplicitStore");
    [Fact] void should_preserve_the_explicit_namespace() => _request.Namespace.ShouldEqual("ExplicitTenant");
    [Fact] void should_preserve_the_explicit_grouping_key() => _request.GroupingKey.ShouldEqual("Explicit/Group");
    [Fact] void should_preserve_the_supplied_context() => _request.Context.ShouldBeSame(_context);
    [Fact] void should_preserve_positive_confidence() => _request.MinimumConfidence.ShouldEqual(0.85);
    [Fact] void should_not_impose_a_local_result_cap() => _request.MaximumResults.ShouldEqual(100000);
}
