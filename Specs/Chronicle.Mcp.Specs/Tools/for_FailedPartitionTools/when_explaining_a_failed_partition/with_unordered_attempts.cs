// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_unordered_attempts : given.a_failed_partition_query
{
    void Establish()
    {
        var selected = AttemptAt(500);
        selected.Occurred = Occurred.AddHours(1);
        selected.Kind = FailureKind.Timeout;
        var tied = AttemptAt(900);
        tied.Occurred = Occurred.AddHours(1);
        _failure.Attempts = [tied, selected, AttemptAt(1000)];
        _page.Data.Events = [EventAt(500)];
    }

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);

    [Fact] void should_choose_by_occurrence_and_break_ties_by_last_input() => _result.Attempt!.SequenceNumber.ShouldEqual(500UL);
    [Fact] void should_read_at_the_selected_attempt() => _pageRequests.Single().From.ShouldEqual(500UL);
    [Fact] void should_include_the_attempt_kind() => _result.Attempt!.Kind.ShouldEqual("Timeout");
    [Fact] void should_include_the_attempt_time() => _result.Attempt!.Occurred.ShouldEqual(Occurred.AddHours(1));
}
