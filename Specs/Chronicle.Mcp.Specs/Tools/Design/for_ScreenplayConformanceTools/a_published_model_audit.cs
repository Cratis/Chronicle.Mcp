// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Tools.Design.Conformance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public abstract class a_published_model_audit : a_namespace_audit
{
    protected const string Entry = "application.play";
    protected const string Published = "Published";
    protected DirectoryInfo _sandbox;
    protected string _root;
    protected ScreenplayModelManifestOptions _manifest;

    protected static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    void Establish()
    {
        _sandbox = Directory.CreateTempSubdirectory("mcp86-tools-");
        _root = Directory.CreateDirectory(Path.Combine(_sandbox.FullName, "private-root-marker")).FullName;
        _manifest = new() { Name = Published, Root = "release", Entry = Entry, Files = [Entry] };
        _options.ModelCatalog.Roots.Add("release", _root);
        _options.ModelCatalog.Models.Add(_manifest);
        SelectPublished();
    }

    void Destroy() => _sandbox.Delete(recursive: true);

    protected Task FreezeCatalog() => _provider.GetServices<IHostedService>().OfType<CatalogStartup>().Single().StartingAsync(Cancellation);

    protected void ConfigureFiles(params string[] paths) => _manifest.Files = paths;

    protected void SelectPublished()
    {
        _arguments.Remove("modelSource");
        Argument("catalogModel", Published);
    }

    protected void AssertRefusal(string code)
    {
        Body.GetProperty("error").GetProperty("code").GetString().ShouldEqual(code);
        Body.GetRawText().Contains("private-", StringComparison.Ordinal).ShouldBeFalse();
        _auditServices.Calls.ShouldEqual(0);
    }

    protected Task AuditInline()
    {
        _arguments.Remove("catalogModel");
        Argument("modelSource", Source);
        return Audit(Cancellation);
    }

    protected void Write(string path, string text)
    {
        var destination = Path.Combine(_root, path);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.WriteAllText(destination, text);
    }
}
