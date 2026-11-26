// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_at_the_failure_limits : given.an_observer_query
{
    static readonly string _stack = new('x', 8192);
    static readonly string _message = new('m', 2048);

    FailedPartitionAttemptExplanation Attempt => _result.Failures.Rows[0].Attempt!;

    void Establish()
    {
        var failure = Failure(new('p', 512));
        failure.Attempts = [new() { Occurred = Occurred, Messages = Enumerable.Repeat(_message, 10), StackTrace = _stack }];
        var omitted = Failure("resolved");
        omitted.IsResolved = true;
        WithFailures(failure, Failure("b"), Failure("c"), Failure("d"), Failure("e"), omitted);
    }

    async Task Because() => _result = await Diagnose();

    [Fact] void should_report_the_fixed_limit() => _result.Failures.Limit.ShouldEqual(5);
    [Fact] void should_return_the_five_eligible_rows() => _result.Failures.Rows.Count.ShouldEqual(5);
    [Fact] void should_not_flag_ineligible_rows_as_truncated() => _result.Failures.Truncated.ShouldBeFalse();
    [Fact] void should_keep_an_innocuous_stack_at_the_limit() => Attempt.StackTrace.ShouldEqual(_stack);
    [Fact] void should_keep_all_ten_messages_at_the_limit() => Attempt.Messages.ShouldEqual(Enumerable.Repeat(_message, 10));
    [Fact] void should_not_flag_exact_limits() => Assert.False(Attempt.MessagesTruncated || Attempt.MessageTextTruncated || Attempt.StackTraceTruncated || _result.IdentifiersTruncated);
    [Fact] void should_not_flag_innocuous_text_as_withheld() => _result.Failures.Rows[0].DiagnosticsRedacted.ShouldBeFalse();
}
