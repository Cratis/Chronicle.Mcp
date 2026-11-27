// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_credential_markers : given.a_failed_partition_query
{
    void Establish()
    {
        var attempt = _failure.Attempts.Single();
        attempt.Messages = ["password policy", new string('m', 2048) + " client secret = synthetic-message"];
        attempt.StackTrace = new string('s', 8192) + " accessToken: synthetic-stack";
    }

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);

    [Fact] void should_screen_the_full_message_before_clipping() => _result.Attempt!.Messages.ShouldEqual("password policy", "[withheld]");
    [Fact] void should_screen_the_full_stack_before_clipping() => _result.Attempt!.StackTrace.ShouldEqual("[withheld]");
    [Fact] void should_report_withholding() => _result.DiagnosticsRedacted.ShouldBeTrue();
    [Fact] void should_not_disclose_raw_diagnostics() => Assert.DoesNotMatch("synthetic-message|synthetic-stack", JsonSerializer.Serialize(_result));
}
