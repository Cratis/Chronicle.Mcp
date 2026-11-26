// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_a_negative_limit : given.a_failed_partition_query
{
    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId, precedingCount: -5);

    [Fact] void should_clamp_to_zero() => (_result.Context.RequestedLimit, _result.Context.Limit, _result.Context.LimitApplied).ShouldEqual((-5, 0, true));
    [Fact] void should_still_make_exactly_one_exact_event_read() => _pageRequests.Single().Count.ShouldEqual(1);
    [Fact] void should_report_no_context_requested() => _result.Context.Status.ShouldEqual("not_requested");
}
