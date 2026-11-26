// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class without_the_exact_event : given.a_failed_partition_query
{
    void Establish() => _page.Data.Events = [EventAt(900), EventAt(4), EventAt(0)];

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId, precedingCount: 2);

    [Fact] void should_report_the_exact_event_is_missing() => _result.Status.ShouldEqual("missing_event");
    [Fact] void should_not_promote_a_neighbor() => _result.FailedEvent.ShouldBeNull();
    [Fact] void should_keep_the_nearest_older_events() => _result.Context.Events.Select(row => row.SequenceNumber).ShouldEqual([4UL, 900UL]);
    [Fact] void should_mark_the_extra_older_event_omitted_even_without_next() => _result.Context.Truncated.ShouldBeTrue();
}
