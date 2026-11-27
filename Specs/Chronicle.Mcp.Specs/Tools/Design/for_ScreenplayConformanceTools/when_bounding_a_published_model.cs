// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design.for_ConfinedCatalogReader;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

[PerFactSpecificationLifecycle]
public class when_bounding_a_published_model : a_published_model_audit
{
    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.SupportsCatalog))]
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

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.SupportsCatalog))]
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
}
