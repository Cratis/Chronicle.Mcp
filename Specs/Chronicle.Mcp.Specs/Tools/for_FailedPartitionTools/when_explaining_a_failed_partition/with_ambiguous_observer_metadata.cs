// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_ambiguous_observer_metadata : given.a_failed_partition_query
{
    void Establish() => _observerInformation = [_observerInformation[0], new() { Id = "observer", EventSequenceId = "other" }];

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);

    [Fact] void should_report_sequence_unavailable() => _result.Status.ShouldEqual("sequence_unavailable");
    [Fact] void should_not_read_an_unverified_sequence() => _pageRequests.ShouldBeEmpty();
}
