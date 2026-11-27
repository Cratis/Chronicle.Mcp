// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design.for_ConfinedCatalogReader;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

[PerFactSpecificationLifecycle]
public class when_catalog_settings_are_unavailable : a_published_model_audit
{
    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.SupportsCatalog))]
    [InlineData("Release", "part.play")]
    [InlineData("release", "./application.play")]
    [InlineData("release", "Application.play")]
    [InlineData("release", Entry)]
    [InlineData("release", "source.txt")]
    async Task should_refuse_a_malformed_catalog_without_breaking_inline(string root, string duplicateOrInvalid)
    {
        Write(Entry, Source);
        _manifest.Root = root;
        ConfigureFiles(Entry, duplicateOrInvalid);

        await RefuseCatalogAndAuditInline();
    }

    [Fact(Skip = "Requires an unsupported host.", SkipType = typeof(LinuxCatalogPlatform), SkipWhen = nameof(LinuxCatalogPlatform.SupportsCatalog))]
    async Task should_refuse_a_configured_catalog_on_an_unsupported_host_without_breaking_inline()
    {
        Write(Entry, Source);

        await RefuseCatalogAndAuditInline();
    }

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.SupportsCatalog))]
    [InlineData("missing")]
    [InlineData("symlink-root")]
    [InlineData("symlink-ancestor")]
    async Task should_not_reopen_a_missing_or_symlinked_root_after_startup(string rootState)
    {
        Write(Entry, Source);
        var unavailable = Path.Combine(_sandbox.FullName, "unavailable");
        _options.ModelCatalog.Roots["release"] = unavailable;
        if (rootState != "missing") Directory.CreateSymbolicLink(unavailable, _root);
        if (rootState == "symlink-ancestor")
        {
            var child = Directory.CreateDirectory(Path.Combine(_root, "child"));
            await File.WriteAllTextAsync(Path.Combine(child.FullName, Entry), Source, Cancellation);
            _options.ModelCatalog.Roots["release"] = Path.Combine(unavailable, "child");
        }

        await FreezeCatalog();
        if (rootState == "missing") Directory.Move(_root, unavailable);
        await Audit(Cancellation);

        AssertRefusal("conformance_catalog_unavailable");
    }

    async Task RefuseCatalogAndAuditInline()
    {
        await Audit(Cancellation);
        AssertRefusal("conformance_catalog_unavailable");

        await AuditInline();
        _result.IsError.ShouldNotEqual(true);
        _auditServices.Calls.ShouldEqual(1);
    }
}
