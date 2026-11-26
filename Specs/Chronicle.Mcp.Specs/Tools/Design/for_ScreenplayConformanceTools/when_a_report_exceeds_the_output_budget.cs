// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using ModelContextProtocol;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_a_report_exceeds_the_output_budget : a_namespace_audit
{
    void Establish()
    {
        _tail = 499;
        Argument("toSequence", 499);
        _metadata.Clear();
        _metadata.AddRange(Enumerable.Range(0, 500).Select(index => Append((ulong)index, $"Legacy{index:D3}{new string('é', 140)}")));
    }

    Task Because() => Audit();

    [Fact]
    void should_bound_the_actual_filtered_result_and_keep_the_complete_counts()
    {
        var wire = JsonSerializer.SerializeToUtf8Bytes(_result, McpJsonUtilities.ProtocolOptions);
        (wire.Length <= 64 * 1024).ShouldBeTrue();
        Body.GetProperty("coverage").GetProperty("appendedEvents").GetInt64().ShouldEqual(500L);
        Body.GetProperty("reportTruncated").GetBoolean().ShouldBeTrue();
        Body.GetProperty("scanCapped").GetBoolean().ShouldBeFalse();
        (Body.GetProperty("omittedFindings").GetInt32() > 0).ShouldBeTrue();
    }

    [Fact]
    void should_still_restrict_disclosure_when_truncated() =>
        JsonSerializer.Serialize(_result, McpJsonUtilities.ProtocolOptions).Contains("private-", StringComparison.Ordinal).ShouldBeFalse();
}
