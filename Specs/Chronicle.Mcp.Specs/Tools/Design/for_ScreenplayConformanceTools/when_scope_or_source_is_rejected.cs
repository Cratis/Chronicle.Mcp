// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using ModelContextProtocol;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_scope_or_source_is_rejected : a_namespace_audit
{
    [Theory]
    [InlineData("eventStore", " ", "conformance_invalid_scope")]
    [InlineData("fromSequence", -1, "conformance_invalid_scope")]
    [InlineData("fromSequence", 3, "conformance_invalid_scope")]
    [InlineData("occurredFrom", "2026-10-01T12:00:00+01:00", "conformance_invalid_scope")]
    [InlineData("modelSource", "import \"features/private-register.play\"", "conformance_invalid_model")]
    [InlineData("modelSource", "? private-malformed", "conformance_invalid_model")]
    public async Task should_refuse_invalid_input_before_accessing_chronicle(string field, object value, string code)
    {
        Argument(field, value);
        await Audit(TestContext.Current.CancellationToken);
        Body.GetProperty("error").GetProperty("code").GetString().ShouldEqual(code);
        JsonSerializer.Serialize(_result, McpJsonUtilities.ProtocolOptions).Contains("private-", StringComparison.Ordinal).ShouldBeFalse();
        _reads.ReceivedCalls().ShouldBeEmpty();
    }
}
