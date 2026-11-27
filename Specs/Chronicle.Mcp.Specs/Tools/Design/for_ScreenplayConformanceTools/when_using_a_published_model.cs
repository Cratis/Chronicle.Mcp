// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Tools.Design.for_ConfinedCatalogReader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

[PerFactSpecificationLifecycle]
public class when_using_a_published_model : a_namespace_audit
{
    const string Entry = "application.play";
    const string Published = "Published";
    const string Nested = "parts/details/register.play";
    DirectoryInfo _sandbox;
    string _root;

    static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    void Establish()
    {
        _sandbox = Directory.CreateTempSubdirectory("mcp86-tools-");
        _root = Directory.CreateDirectory(Path.Combine(_sandbox.FullName, "private-root-marker")).FullName;
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ModelCatalog:Roots:release"] = _root,
            [$"ModelCatalog:Models:{Published}:Root"] = "release",
            [$"ModelCatalog:Models:{Published}:Entry"] = Entry,
            [$"ModelCatalog:Models:{Published}:Files:0"] = Entry
        }).Build();
        McpOptionsBinding.Bind(settings, _options);
        SelectPublished();
    }

    void Destroy() => _sandbox.Delete(recursive: true);

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    [InlineData(false)]
    [InlineData(true)]
    async Task should_produce_the_same_evidence_as_the_equivalent_inline_model(bool split)
    {
        if (split)
        {
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
        _arguments.Remove("catalogModel");
        Argument("modelSource", Source);
        await Audit(Cancellation);

        JsonNode.DeepEquals(published, JsonNode.Parse(Body.GetRawText())).ShouldBeTrue();
    }

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    [InlineData("Release", "part.play")]
    [InlineData("release", "./application.play")]
    [InlineData("release", "Application.play")]
    [InlineData("release", Entry)]
    [InlineData("release", "source.txt")]
    async Task should_refuse_a_malformed_catalog_without_breaking_inline(string root, string duplicateOrInvalid)
    {
        Write(Entry, Source);
        _options.ModelCatalog.Models[Published].Root = root;
        ConfigureFiles(Entry, duplicateOrInvalid);

        await Audit(Cancellation);
        AssertRefusal("conformance_catalog_unavailable");

        await AuditInline();
    }

    [Fact(Skip = "Requires an unsupported host.", SkipType = typeof(LinuxCatalogPlatform), SkipWhen = nameof(LinuxCatalogPlatform.IsLinux))]
    async Task should_refuse_a_configured_catalog_on_an_unsupported_host_without_breaking_inline()
    {
        Write(Entry, Source);
        await Audit(Cancellation);
        AssertRefusal("conformance_catalog_unavailable");

        await AuditInline();
    }

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
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

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
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

    [Fact(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    async Task should_refuse_invalid_utf8_and_allow_a_retry_after_the_file_is_replaced()
    {
        File.WriteAllBytes(Path.Combine(_root, Entry), [0xc0, 0x80]);
        await Audit(Cancellation);
        AssertRefusal("conformance_invalid_model");

        Write(Entry, Source);
        await Audit(Cancellation);
        _result.IsError.ShouldNotEqual(true);
    }

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    [InlineData(16)]
    [InlineData(17)]
    async Task should_cap_the_complete_file_manifest_including_the_entry(int fileCount)
    {
        Write(Entry, $"{Source}\nimport \"parts/*.play\"\n");
        var dependencies = Enumerable.Range(1, fileCount - 1).Select(number => $"parts/{number}.play").ToArray();
        foreach (var dependency in dependencies) Write(dependency, "// published dependency\n");
        ConfigureFiles([Entry, .. dependencies]);

        await Audit(Cancellation);

        if (fileCount == 16) _result.IsError.ShouldNotEqual(true);
        else AssertRefusal("conformance_catalog_unavailable");
    }

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    [InlineData(65536)]
    [InlineData(65537)]
    async Task should_enforce_the_aggregate_byte_budget_across_multiple_files(int byteCount)
    {
        var application = $"{Source}\nimport \"part.play\"\n";
        Write(Entry, application);
        var required = byteCount - application.Length;
        var padding = "//" + new string('x', 67) + "\n";
        var lines = string.Concat(Enumerable.Repeat(padding, (required - 2) / padding.Length));
        Write("part.play", lines + "//" + new string('x', required - lines.Length - 2));
        ConfigureFiles(Entry, "part.play");

        await Audit(Cancellation);

        if (byteCount == 65536) _result.IsError.ShouldNotEqual(true);
        else AssertRefusal("conformance_model_limit");
    }

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
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
            Directory.CreateDirectory(Path.Combine(_root, "child"));
            _options.ModelCatalog.Roots["release"] = Path.Combine(unavailable, "child");
        }

        await _provider.GetRequiredService<IAsyncStartupValidator>().ValidateAsync(Cancellation);
        if (rootState == "missing") Directory.Move(_root, unavailable);
        await Audit(Cancellation);

        AssertRefusal("conformance_catalog_unavailable");
    }

    [Fact(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    async Task should_use_exact_names_and_keep_the_startup_manifest()
    {
        Write(Entry, Source);
        await _provider.GetRequiredService<IAsyncStartupValidator>().ValidateAsync(Cancellation);
        ConfigureFiles("private-document-marker.play");
        Argument("catalogModel", "published");
        await Audit(Cancellation);
        AssertRefusal("conformance_catalog_unavailable");

        SelectPublished();
        await Audit(Cancellation);
        _result.IsError.ShouldNotEqual(true);
    }

    [Fact(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    async Task should_release_the_admission_slot_after_cancellation()
    {
        Write(Entry, Source);
        await Audit(new CancellationToken(canceled: true));
        AssertRefusal("conformance_cancelled");

        await Audit(Cancellation);
        _result.IsError.ShouldNotEqual(true);
    }

    void ConfigureFiles(params string[] paths) => _options.ModelCatalog.Models[Published].Files = paths;

    void SelectPublished()
    {
        _arguments.Remove("modelSource");
        Argument("catalogModel", Published);
    }

    void AssertRefusal(string code)
    {
        Body.GetProperty("error").GetProperty("code").GetString().ShouldEqual(code);
        Body.GetRawText().Contains("private-", StringComparison.Ordinal).ShouldBeFalse();
        _auditServices.Calls.ShouldEqual(0);
    }

    async Task AuditInline()
    {
        _arguments.Remove("catalogModel");
        Argument("modelSource", Source);
        await Audit(Cancellation);
        _result.IsError.ShouldNotEqual(true);
        _auditServices.Calls.ShouldEqual(1);
    }

    void Write(string path, string text)
    {
        var destination = Path.Combine(_root, path);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.WriteAllText(destination, text);
    }
}
