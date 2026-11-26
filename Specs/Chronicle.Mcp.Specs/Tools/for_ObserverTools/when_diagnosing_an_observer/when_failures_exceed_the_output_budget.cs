// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_failures_exceed_the_output_budget : given.an_observer_query
{
    static readonly string _boundary = new('m', 2047);
    static readonly string _exactMessage = new('e', 2048);
    static readonly string _partitionPrefix = new('p', 511);
    Guid _retainedId;

    ObserverFailureEvidence First => _result.Failures.Rows[0];
    FailedPartitionAttemptExplanation Attempt => First.Attempt!;

    void Establish()
    {
        var retained = Failure(_partitionPrefix + "😀");
        _retainedId = retained.Id;
        retained.Attempts = [new() { Occurred = Occurred, Messages = ["older-attempt"] }, Latest(), new() { Occurred = Occurred.AddDays(1), Messages = ["earlier-tie"] }];
        retained.Attempts = retained.Attempts.Append(Latest());
        var resolved = Failure("resolved");
        resolved.IsResolved = true;
        var unrelated = Failure("unrelated");
        unrelated.ObserverId = ObserverId.ToUpperInvariant();
        WithFailures([resolved, unrelated, retained, .. Enumerable.Range(2, 6).Select(index => Failure($"partition-{index}"))]);
    }

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_cap_eligible_rows_in_server_order() => _result.Failures.Rows.Select(row => row.Partition).ShouldEqual([_partitionPrefix, "partition-2", "partition-3", "partition-4", "partition-5"]);
    [Fact] void should_flag_the_lookahead_row() => _result.Failures.Truncated.ShouldBeTrue();
    [Fact] void should_flag_clipped_identifiers() => _result.IdentifiersTruncated.ShouldBeTrue();
    [Fact] void should_keep_the_exact_failure_id() => First.Id.ShouldEqual(_retainedId);
    [Fact] void should_keep_the_latest_occurrence() => Attempt.Occurred.ShouldEqual(Occurred.AddDays(1));
    [Fact] void should_keep_the_latest_kind() => Attempt.Kind.ShouldEqual("Timeout");
    [Fact] void should_preserve_the_recorded_sentinel() => Attempt.SequenceNumber.ShouldEqual(ulong.MaxValue);
    [Fact] void should_not_split_a_surrogate_pair() => Attempt.Messages[0].ShouldEqual(_boundary);
    [Fact] void should_keep_messages_at_the_exact_limit() => Attempt.Messages[1].ShouldEqual(_exactMessage);
    [Fact] void should_withhold_a_message_before_clipping() => Attempt.Messages[2].ShouldEqual("[withheld]");
    [Fact] void should_withhold_a_stack_before_clipping() => Attempt.StackTrace.ShouldEqual("[withheld]");
    [Fact] void should_flag_credential_withholding() => First.DiagnosticsRedacted.ShouldBeTrue();
    [Fact] void should_flag_source_text_limits() => Assert.True(Attempt.MessageTextTruncated && Attempt.StackTraceTruncated);
    [Fact] void should_cap_the_message_count() => Attempt.Messages.Count.ShouldEqual(10);
    [Fact] void should_flag_omitted_messages() => Attempt.MessagesTruncated.ShouldBeTrue();
    [Fact] void should_report_an_absent_attempt() => _result.Failures.Rows[1].AttemptStatus.ShouldEqual("no_attempt");
    [Fact] void should_not_disclose_raw_text_or_attempt_history() => Assert.DoesNotMatch("synthetic-marker|older-attempt|earlier-tie", JsonSerializer.Serialize(_result));
    [Fact] void should_only_use_the_allowed_reads() => ShouldOnlyRead(2, 1, 1);

    static FailedPartitionAttempt Latest() => new()
    {
        Occurred = Occurred.AddDays(1),
        Kind = FailureKind.Timeout,
        SequenceNumber = ulong.MaxValue,
        Messages = [_boundary + "😀", _exactMessage, _exactMessage + " ToKeN = synthetic-marker", .. Enumerable.Repeat("innocuous", 8)],
        StackTrace = new string('x', 8192) + " Authorization: Bearer synthetic-marker"
    };
}
