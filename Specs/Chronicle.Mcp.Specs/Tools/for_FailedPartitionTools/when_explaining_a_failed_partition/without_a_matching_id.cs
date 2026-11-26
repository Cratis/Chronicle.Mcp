// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class without_a_matching_id : given.a_failed_partition_query
{
    void Establish() => _failure.Id = Guid.Empty;

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);

    [Fact] void should_report_not_found() => _result.Status.ShouldEqual("not_found");
    [Fact] void should_not_search_other_scopes() => _failureRequests.Count.ShouldEqual(1);
    [Fact] void should_not_return_unrelated_failure_data() => (_result.Attempt, _result.ObserverId, _result.Partition).ShouldEqual((null, null, null));
    [Fact] void should_not_resolve_a_sequence() => _observerRequests.ShouldBeEmpty();
    [Fact] void should_not_read_events() => _pageRequests.ShouldBeEmpty();
}
