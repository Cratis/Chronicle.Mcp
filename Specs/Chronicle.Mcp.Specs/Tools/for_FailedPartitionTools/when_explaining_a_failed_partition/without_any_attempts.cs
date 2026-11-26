// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class without_any_attempts : given.a_failed_partition_query
{
    void Establish() => _failure.Attempts = [];

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);

    [Fact] void should_report_no_attempt() => _result.Status.ShouldEqual("no_attempt");
    [Fact] void should_not_read_events() => _pageRequests.ShouldBeEmpty();
}
