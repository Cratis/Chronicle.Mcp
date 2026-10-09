// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Mcp.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.Specs.given;

public class a_policy : Specification
{
    protected const string Store = "store-private";
    protected const string Namespace = "tenant-private";
    protected const string Principal = "operator-private";
    protected const string Target = "621d846d-56b7-49a4-8c1b-7703c6955c11";
    protected static readonly DateTimeOffset Now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    protected McpServerOptions _settings;
    protected MutationGrant _grant;
    protected ToolCatalog _catalog;
    protected MutationPolicy _policy;
    protected TimeProvider _clock;
    protected ChronicleConnectionConfiguration _configuration;
    protected IDictionary<string, JsonElement> _arguments;
    protected IOptionsMonitor<McpServerOptions> _monitor;

    void Establish()
    {
        _catalog = new(typeof(ToolCatalog).Assembly);
        _policy = new(_catalog);
        _clock = Substitute.For<TimeProvider>();
        _clock.GetUtcNow().Returns(Now);
        _grant = new() { Principal = Principal, EventStore = Store, Namespace = Namespace, Operations = ["stop_job", "resume_job", "delete_job"], Targets = [Target] };
        _settings = new()
        {
            Profile = DeploymentProfile.Mutation,
            UseCliConfiguration = false,
            EventStore = Store,
            Namespace = Namespace,
            Authorization = new() { Principal = Principal, ExpiresAt = Now.AddHours(1), Grants = [_grant] }
        };
        _configuration = new(Options.Create(_settings));
        _arguments = new Dictionary<string, JsonElement>(StringComparer.Ordinal)
        {
            ["jobId"] = JsonSerializer.SerializeToElement(Target),
            ["eventStore"] = JsonSerializer.SerializeToElement(Store),
            ["namespace"] = JsonSerializer.SerializeToElement(Namespace)
        };
        _monitor = Substitute.For<IOptionsMonitor<McpServerOptions>>();
        _monitor.CurrentValue.Returns(_settings);
    }
}
