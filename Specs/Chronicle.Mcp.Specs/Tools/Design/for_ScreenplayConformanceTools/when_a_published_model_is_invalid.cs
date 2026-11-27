// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design.for_ConfinedCatalogReader;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

[PerFactSpecificationLifecycle]
public class when_a_published_model_is_invalid : a_published_model_audit
{
    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.SupportsCatalog))]
    [InlineData("/part.play", true, true)]
    [InlineData("parts/../part.play", true, true)]
    [InlineData("missing.play", true, false)]
    [InlineData("*.missing.play", true, false)]
    [InlineData("part.play", false, true)]
    [InlineData("part.play", true, false)]
    async Task should_refuse_imports_outside_the_closed_manifest(string import, bool referenced, bool declared)
    {
        Write(Entry, referenced ? $"{Source}\nimport \"{import}\"\n" : Source);
        Write("part.play", "// private-document-marker\n");
        if (declared) ConfigureFiles(Entry, "part.play");

        await Audit(Cancellation);

        AssertRefusal("conformance_invalid_model");
    }

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.SupportsCatalog))]
    [InlineData("catalog/private-document-marker")]
    [InlineData("npm:@sample/audit-extension")]
    [InlineData("oci://example.invalid/sample/contract")]
    async Task should_refuse_an_extension_in_an_imported_document_before_semantic_binding(string reference)
    {
        Write(Entry, $"{Source}\nimport \"part.play\"\n");
        Write("part.play", $"system Outside\n  extension \"{reference}\" version 2\n");
        ConfigureFiles(Entry, "part.play");

        await Audit(Cancellation);

        AssertRefusal("conformance_invalid_model");
        Body.GetProperty("diagnostics").EnumerateArray().ShouldBeEmpty();
    }

    [Fact(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.SupportsCatalog))]
    async Task should_refuse_invalid_utf8_and_allow_a_retry_after_the_file_is_replaced()
    {
        File.WriteAllBytes(Path.Combine(_root, Entry), [0xc0, 0x80]);
        await Audit(Cancellation);
        AssertRefusal("conformance_invalid_model");

        Write(Entry, Source);
        await Audit(Cancellation);
        _result.IsError.ShouldNotEqual(true);
    }
}
