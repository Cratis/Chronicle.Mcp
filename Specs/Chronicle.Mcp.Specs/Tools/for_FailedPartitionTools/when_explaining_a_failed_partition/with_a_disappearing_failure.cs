// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_a_disappearing_failure : given.a_failed_partition_query
{
    void Establish() => _finalFailures = [];

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);

    [Fact] void should_report_disappearance() => _result.Status.ShouldEqual("disappeared");
    [Fact] void should_not_show_stale_events() => _result.FailedEvent.ShouldBeNull();
    [Fact] void should_not_show_stale_context() => _result.Context.Events.ShouldBeEmpty();
    [Fact] void should_use_only_the_three_read_endpoints() => ShouldOnlyUseTheFourReads();
    [Fact] void should_not_attempt_recovery() => ShouldNotMutate();
}
