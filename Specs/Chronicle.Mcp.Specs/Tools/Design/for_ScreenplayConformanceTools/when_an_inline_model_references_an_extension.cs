// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Screenplay;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_an_inline_model_references_an_extension : a_namespace_audit
{
    [Theory]
    [InlineData("catalog/sample")]
    [InlineData("npm:@sample/audit-extension")]
    [InlineData("oci://example.invalid/sample/contract")]
    public async Task should_refuse_external_resolution_before_binding_or_reading_chronicle(string reference)
    {
        var model = Source.Replace("module Library", $"system Outside\n  extension \"{reference}\" version 2\n\nmodule Library", StringComparison.Ordinal);
        new ScreenplayCompiler().Parse(model, "application.play").Value!.Systems.Single().Extension!.Reference.ShouldEqual(reference);
        Argument("modelSource", model);

        await Audit(TestContext.Current.CancellationToken);

        Body.GetProperty("error").GetProperty("code").GetString().ShouldEqual("conformance_invalid_model");
        Body.GetProperty("diagnostics").EnumerateArray().ShouldBeEmpty();
        _auditServices.Calls.ShouldEqual(0);
    }
}
