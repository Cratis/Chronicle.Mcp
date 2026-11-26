// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_a_limit_above_the_cap : given.a_failed_partition_query
{
    void Establish()
    {
        _page.Data.Events = Enumerable.Range(0, 51).Select(offset => EventAt(1000UL - (ulong)offset));
        _page.Data.Next = 949;
    }

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId, precedingCount: 100);

    [Fact] void should_clamp_at_fifty() => (_result.Context.RequestedLimit, _result.Context.Limit, _result.Context.LimitApplied).ShouldEqual((100, 50, true));
    [Fact] void should_request_one_page_bounded_to_fifty_one() => _pageRequests.Single().Count.ShouldEqual(51);
    [Fact] void should_include_fifty_predecessors() => _result.Context.Events.Count.ShouldEqual(50);
    [Fact] void should_show_that_next_indicates_omitted_history() => _result.Context.Truncated.ShouldBeTrue();
}
