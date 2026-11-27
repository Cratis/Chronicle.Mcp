// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;
using Cratis.Chronicle.Mcp.Tools.Design.for_ConfinedCatalogReader;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

[PerFactSpecificationLifecycle]
public class when_using_a_published_model : a_published_model_audit
{
    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.SupportsCatalog))]
    [InlineData(false)]
    [InlineData(true)]
    async Task should_produce_the_same_evidence_as_the_equivalent_inline_model(bool split)
    {
        if (split)
        {
            const string Nested = "parts/details/register.play";
            ConfigureFiles(Entry, "parts/feature.play", Nested);
            Write(Entry, "domain Library\nmodule Library\n  feature Registration\n    import \"parts/feature.play\"\n");
            Write("parts/feature.play", "import \"details/*.play\"\n");
            var slice = Source.Split('\n').Skip(3).Select(line => line.Length >= 4 ? line[4..] : line);
            Write(Nested, string.Join('\n', slice));
        }
        else
        {
            Write(Entry, Source);
        }

        await Audit(Cancellation);
        var published = JsonNode.Parse(Body.GetRawText());
        _result.IsError.ShouldNotEqual(true);
        _auditServices.Calls.ShouldEqual(1);
        await AuditInline();

        JsonNode.DeepEquals(published, JsonNode.Parse(Body.GetRawText())).ShouldBeTrue();
    }

    [Fact(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.SupportsCatalog))]
    async Task should_use_exact_names_and_keep_the_startup_manifest()
    {
        Write(Entry, Source);
        await FreezeCatalog();
        ConfigureFiles("private-document-marker.play");
        Argument("catalogModel", "published");
        await Audit(Cancellation);
        AssertRefusal("conformance_catalog_unavailable");

        SelectPublished();
        await Audit(Cancellation);
        _result.IsError.ShouldNotEqual(true);
    }

    [Fact(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.SupportsCatalog))]
    async Task should_release_the_admission_slot_after_cancellation()
    {
        Write(Entry, Source);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Cancellation);

        // The tool resolves the catalog after admission; its first document read observes cancellation.
        _onCatalogCreated = cancellation.Cancel;
        await Audit(cancellation.Token);
        AssertRefusal("conformance_cancelled");

        await Audit(Cancellation);
        _result.IsError.ShouldNotEqual(true);
        _auditServices.Calls.ShouldEqual(1);
    }
}
