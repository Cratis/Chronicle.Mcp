// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

[PerFactSpecificationLifecycle]
public class when_selecting_a_model : a_namespace_audit
{
    [Fact]
    public async Task should_audit_a_valid_inline_model_without_a_catalog()
    {
        await Audit(TestContext.Current.CancellationToken);

        _result.IsError.ShouldNotEqual(true);
        _auditServices.Calls.ShouldEqual(1);
    }

    [Theory]
    [InlineData(null, null, "conformance_model_limit")]
    [InlineData(null, "missing", "conformance_catalog_unavailable")]
    [InlineData(null, "/private-model-marker.play", "conformance_catalog_unavailable")]
    [InlineData("", "published", "conformance_catalog_unavailable")]
    [InlineData(Source, "", "conformance_catalog_unavailable")]
    public async Task should_refuse_without_resolving_chronicle(string? inline, string? name, string code)
    {
        _arguments.Remove("modelSource");
        if (inline is not null) Argument("modelSource", inline);
        if (name is not null) Argument("catalogModel", name);

        await Audit(TestContext.Current.CancellationToken);

        Body.GetProperty("error").GetProperty("code").GetString().ShouldEqual(code);
        Body.GetRawText().Contains("private-model-marker", StringComparison.Ordinal).ShouldBeFalse();
        _auditServices.Calls.ShouldEqual(0);
    }
}
