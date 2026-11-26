// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class without_the_observer : given.a_failed_partition_query
{
    void Establish() => _observerInformation[0].Id = "unrelated-observer";

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);

    [Fact] void should_report_sequence_unavailable() => _result.Status.ShouldEqual("sequence_unavailable");
    [Fact] void should_not_read_events() => _pageRequests.ShouldBeEmpty();
}
