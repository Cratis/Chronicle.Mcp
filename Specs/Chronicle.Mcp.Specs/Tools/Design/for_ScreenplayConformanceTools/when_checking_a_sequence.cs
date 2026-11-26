// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_checking_a_sequence : a_namespace_audit
{
    [Fact]
    public async Task should_report_empty_evidence_for_an_existing_empty_sequence()
    {
        _tail = ulong.MaxValue - 2;
        await Audit();
        var sequence = Body.GetProperty("coverage").GetProperty("sequence");
        sequence.GetProperty("complete").GetBoolean().ShouldBeTrue();
        _pages.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("EVENT-LOG")]
    [InlineData("missing")]
    public async Task should_not_treat_an_unknown_or_differently_cased_sequence_as_empty(string sequence)
    {
        Argument("sequence", sequence);
        await Audit();
        Body.GetProperty("error").GetProperty("code").GetString().ShouldEqual("conformance_target_not_found");
        _pages.ShouldBeEmpty();
    }
}
