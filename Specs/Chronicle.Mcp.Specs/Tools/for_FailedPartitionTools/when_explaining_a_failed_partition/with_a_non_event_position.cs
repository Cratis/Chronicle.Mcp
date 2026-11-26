// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_a_non_event_position : given.a_failed_partition_query
{
    FailedPartitionExplanation[] _results;

    async Task Because()
    {
        _results = new FailedPartitionExplanation[3];
        for (var offset = 0UL; offset < 3; offset++)
        {
            _failure.Attempts = [AttemptAt(ulong.MaxValue - offset)];
            _results[offset] = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);
        }
    }

    [Fact] void should_report_no_event_for_all_three_sentinels() => _results.Select(result => result.Status).ShouldEqual(["no_event", "no_event", "no_event"]);
    [Fact] void should_not_read_events() => _pageRequests.ShouldBeEmpty();
}
