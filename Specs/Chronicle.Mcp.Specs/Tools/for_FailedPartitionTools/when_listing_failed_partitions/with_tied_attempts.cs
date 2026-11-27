// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_listing_failed_partitions;

public class with_tied_attempts : given.a_failed_partition_query
{
    FailedPartitionDescriptor _row;

    void Establish()
    {
        var selected = AttemptAt(2000);
        selected.Messages = ["token expired", "clientSecret=synthetic-message", "Sample failure"];
        selected.StackTrace = "api_key: synthetic-stack";
        _failure.Attempts = [AttemptAt(1000), selected];
    }

    async Task Because() => _row = (await FailedPartitionTools.ListFailedPartitions(_services, _configuration)).Single();

    [Fact] void should_pick_the_last_recorded_attempt_at_the_same_time() => _row.LastFailedSequenceNumber.ShouldEqual(2000UL);
    [Fact] void should_screen_messages_before_joining() => _row.LastFailureReason.ShouldEqual("token expired | [withheld] | Sample failure");
    [Fact] void should_report_withholding() => _row.DiagnosticsRedacted.ShouldBeTrue();
    [Fact] void should_not_disclose_raw_diagnostics() => Assert.DoesNotMatch("synthetic-message|synthetic-stack", JsonSerializer.Serialize(_row));
}
