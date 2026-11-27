// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Connection;
using Cratis.Chronicle.Mcp.Tools.Design.Conformance;
using Cratis.Chronicle.Mcp.Tools.Design.for_ConfinedCatalogReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ConformanceModelCatalog;

public class when_the_host_starts : Specification
{
    const string Entry = "application.play";
    DirectoryInfo _sandbox;
    string _root;
    IHost _host;
    int _snapshots;

    void Establish()
    {
        _sandbox = Directory.CreateTempSubdirectory("mcp86-host-");
        _root = Directory.CreateDirectory(Path.Combine(_sandbox.FullName, "active")).FullName;
        File.WriteAllText(Path.Combine(_root, Entry), "domain Library\n");
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { DisableDefaults = true });
        builder.Services.Configure<McpServerOptions>(options =>
        {
            options.ModelCatalog.Roots.Add("release", _root);
            options.ModelCatalog.Models.Add(new() { Name = "Library", Root = "release", Entry = Entry, Files = [Entry] });
        });
        builder.Services.AddSingleton(ObserveSnapshot);
        builder.Services.AddChronicleMcpConnection();
        _host = builder.Build();
    }

    void Destroy()
    {
        _host.Dispose();
        _sandbox.Delete(recursive: true);
    }

    [Fact]
    async Task should_capture_the_catalog_during_start_before_any_audit_resolves_it()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        _snapshots.ShouldEqual(0);
        await _host.StartAsync(cancellationToken);
        try
        {
            _snapshots.ShouldEqual(1);
            Directory.Move(_root, Path.Combine(_sandbox.FullName, "pinned"));
            Directory.CreateDirectory(_root);

            var result = _host.Services.GetRequiredService<ConformanceModelCatalog>().Load("Library", cancellationToken);
            if (LinuxCatalogPlatform.SupportsCatalog)
            {
                result.Error.ShouldBeNull();
                result.Documents!.Single().Text.ShouldEqual("domain Library\n");
            }
            else
            {
                result.Error.ShouldEqual("conformance_catalog_unavailable");
            }
        }
        finally
        {
            await _host.StopAsync(cancellationToken);
        }
    }

    ConformanceModelCatalog ObserveSnapshot(IServiceProvider services)
    {
        _snapshots++;
        return new(services.GetRequiredService<IOptions<McpServerOptions>>());
    }
}
