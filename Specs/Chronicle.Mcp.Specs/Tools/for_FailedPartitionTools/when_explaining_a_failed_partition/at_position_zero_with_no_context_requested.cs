// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class at_position_zero_with_no_context_requested : given.a_failed_partition_query
{
    void Establish()
    {
        _failure.Attempts = [AttemptAt(0)];
        _page.Data.Events = [EventAt(0)];
    }

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId, precedingCount: 0);

    [Fact] void should_explain_zero_as_a_real_event() => _result.FailedEvent!.SequenceNumber.ShouldEqual(0UL);
    [Fact] void should_look_up_only_zero() => (_pageRequests.Single().From, _pageRequests.Single().Until, _pageRequests.Single().Count).ShouldEqual(((ulong?)0, (ulong?)0, 1));
    [Fact] void should_report_no_context_requested() => _result.Context.Status.ShouldEqual("not_requested");
    [Fact] void should_not_return_context_events() => _result.Context.Events.ShouldBeEmpty();
    [Fact] void should_not_clamp_zero() => _result.Context.LimitApplied.ShouldBeFalse();
}
