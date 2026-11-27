// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_listing_failed_partitions;

public class with_only_the_stack_withheld : given.a_failed_partition_query
{
    FailedPartitionDescriptor _row;

    void Establish() => _failure.Attempts.Single().StackTrace = "refreshToken=synthetic-stack";

    async Task Because() => _row = (await FailedPartitionTools.ListFailedPartitions(_services, _configuration)).Single();

    [Fact] void should_keep_the_clean_reason() => _row.LastFailureReason.ShouldEqual("Sample failure");
    [Fact] void should_count_a_withheld_stack() => _row.DiagnosticsRedacted.ShouldBeTrue();
    [Fact] void should_not_expose_the_stack() => Assert.DoesNotContain("synthetic-stack", JsonSerializer.Serialize(_row), StringComparison.Ordinal);
}
