// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Tools.Design.Conformance;
using Microsoft.Extensions.Configuration;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

[PerFactSpecificationLifecycle]
public class when_binding_a_json_model_catalog : a_published_model_audit
{
    const string Manifest = "{\"Name\":\"Published\",\"Root\":\"release\",\"Entry\":\"application.play\",\"Files\":[\"application.play\"]}";
    const string CaseVariant = "{\"Name\":\"published\",\"Root\":\"release\",\"Entry\":\"application.play\",\"Files\":[\"application.play\"]}";
    const string SplitObject = "{\"Published\":{\"Root\":\"release\"},\"published\":{\"Name\":\"Published\",\"Entry\":\"application.play\",\"Files\":[\"application.play\"]}}";
    const string SplitArray = "[{\"Name\":\"Published\",\"Root\":\"release\"},{\"Name\":\"published\",\"Entry\":\"application.play\",\"Files\":[\"application.play\"]}]";

    [Fact]
    void should_bind_the_root_name_entry_and_files_from_the_deployment_section()
    {
        const string Root = "/models/library";
        Bind($"[{Manifest}]", Root);

        _options.ModelCatalog.Roots["release"].ShouldEqual(Root);
        var model = _options.ModelCatalog.Models.Single();
        model.Name.ShouldEqual(Published);
        model.Root.ShouldEqual("release");
        model.Entry.ShouldEqual(Entry);
        model.Files.Single().ShouldEqual(Entry);
        CatalogManifests.Freeze(_options.ModelCatalog).ShouldNotBeNull();
    }

    [Theory]
    [InlineData(SplitObject)]
    [InlineData(SplitArray)]
    [InlineData("[" + Manifest + "," + CaseVariant + "]")]
    [InlineData("[" + Manifest + "," + Manifest + "]")]
    async Task should_refuse_ambiguous_declarations_without_breaking_inline(string manifests)
    {
        Write(Entry, Source);
        Bind(manifests, _root);
        CatalogManifests.Freeze(_options.ModelCatalog).ShouldBeNull();

        await Audit(Cancellation);
        AssertRefusal("conformance_catalog_unavailable");

        await AuditInline();
        _result.IsError.ShouldNotEqual(true);
        _auditServices.Calls.ShouldEqual(1);
    }

    void Bind(string models, string root)
    {
        var catalog = $"{{\"Roots\":{{\"release\":{JsonSerializer.Serialize(root)}}},\"Models\":{models}}}";
        var json = $"{{\"Cratis\":{{\"Chronicle\":{{\"Mcp\":{{\"ModelCatalog\":{catalog}}}}}}}}}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var configuration = new ConfigurationBuilder().AddJsonStream(stream).Build();
        _options.ModelCatalog = new();
        McpOptionsBinding.Bind(configuration.GetSection(McpOptionsBinding.SectionPath), _options);
    }
}
