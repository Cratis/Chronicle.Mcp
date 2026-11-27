// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_listing_failed_partitions;

public class with_clean_diagnostics : given.a_failed_partition_tool_call
{
    JsonElement _row;

    async Task Because()
    {
        var result = await Invoke("list_failed_partitions");
        _row = JsonSerializer.Deserialize<JsonElement>(result.Content.OfType<TextContentBlock>().Single().Text)[0];
    }

    [Fact] void should_preserve_the_reason() => _row.GetProperty("lastFailureReason").GetString().ShouldEqual("Sample failure");
    [Fact] void should_not_report_withholding() => _row.GetProperty("diagnosticsRedacted").GetBoolean().ShouldBeFalse();
    [Fact] void should_not_expose_a_clean_stack() => Assert.DoesNotContain("Sample stack", _row.GetRawText(), StringComparison.Ordinal);
    [Fact] void should_use_the_mcp_token_for_the_read() => _callContexts.Single().CancellationToken.ShouldEqual(_cancellation.Token);
}
