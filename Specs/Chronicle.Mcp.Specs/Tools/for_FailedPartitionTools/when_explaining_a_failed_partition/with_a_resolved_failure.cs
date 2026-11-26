// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_a_resolved_failure : given.a_failed_partition_query
{
    void Establish() => _failure.IsResolved = true;

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);

    [Fact] void should_report_resolution() => _result.Status.ShouldEqual("resolved");
    [Fact] void should_include_the_resolution_state() => _result.IsResolved.ShouldEqual(true);
    [Fact] void should_not_read_events() => _pageRequests.ShouldBeEmpty();
}
