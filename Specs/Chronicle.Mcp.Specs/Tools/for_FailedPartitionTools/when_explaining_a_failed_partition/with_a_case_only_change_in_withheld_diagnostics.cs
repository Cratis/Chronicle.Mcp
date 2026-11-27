// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_a_case_only_change_in_withheld_diagnostics : given.a_failed_partition_query
{
    void Establish()
    {
        const string diagnostic = "password: synthetic-marker";
        _failure.Attempts.Single().Messages = [diagnostic];
        var changed = AttemptAt(1000);
        changed.Messages = [diagnostic.ToUpperInvariant()];
        _finalFailures = [new FailedPartition { Id = FailureId, ObserverId = "observer", Partition = "partition", Attempts = [changed] }];
    }

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);

    [Fact] void should_report_the_case_only_change() => _result.Status.ShouldEqual("changed");
    [Fact] void should_keep_only_the_screened_diagnostic() => _result.Attempt!.Messages.ShouldEqual<string>("[withheld]");
    [Fact] void should_not_offer_event_evidence() => _result.FailedEvent.ShouldBeNull();
    [Fact] void should_not_offer_preceding_events() => _result.Context.Events.ShouldBeEmpty();
}
