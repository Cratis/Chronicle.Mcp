// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_a_partition_is_quarantined : given.an_observer_query
{
    void Establish()
    {
        var failure = Failure("quarantined-partition");
        failure.IsQuarantined = true;
        var unknown = Failure("unknown-partition");
        unknown.IsQuarantined = null;
        WithFailures(failure, unknown);
    }

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_not_infer_observer_quarantine() => _result.Observer!.IsQuarantined.ShouldEqual(false);
    [Fact] void should_report_active_even_when_replayable() => _result.Observer!.RunningState.ShouldEqual("Active");
    [Fact] void should_report_partition_quarantine_separately() => _result.Failures.Rows[0].IsQuarantined.ShouldEqual(true);
    [Fact] void should_preserve_unknown_partition_quarantine() => _result.Failures.Rows[1].IsQuarantined.ShouldBeNull();
    [Fact] void should_only_use_the_allowed_reads() => ShouldOnlyRead(2, 1, 1);
}
