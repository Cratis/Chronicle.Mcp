// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Authorization;
using Cratis.Chronicle.Mcp.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Connection.for_ChronicleMcpServiceCollectionExtensions.given;

public class a_delayed_connection : Configuration.for_ChronicleConnectionConfiguration.given.a_connection_configuration
{
    protected readonly DateTimeOffset _now = new(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
    protected TimeProvider _clock;
    protected ServiceProvider _provider;
    protected Exception? _startupError;
    protected Exception? _connectionError;

    void Establish()
    {
        _context.TokenExpiry = _now.AddMinutes(2).ToString("O");
        EnableCli();
        _clock = Substitute.For<TimeProvider>();
        _clock.GetUtcNow().Returns(_now);
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Profile"] = "Mutation",
            ["ConnectionString"] = _context.Server,
            ["UseCliConfiguration"] = "true",
            ["CliConfigurationPath"] = _options.CliConfigurationPath,
            ["UseDevelopmentCredentials"] = "true",
            ["Authorization:ExpiresAt"] = _now.AddHours(1).ToString("O"),
            ["Authorization:Allowlist:0:EventStore"] = "orders",
            ["Authorization:Allowlist:0:Namespace"] = "support",
            ["Authorization:Allowlist:0:Operations:0"] = "stop_job"
        }).Build();
        _provider = new ServiceCollection()
            .AddSingleton(_clock)
            .AddSingleton(Substitute.For<IHostApplicationLifetime>())
            .AddDeploymentPolicy(settings)
            .AddChronicleMcpConnection()
            .BuildServiceProvider();
    }

    protected Task Start() => _provider.GetServices<IHostedService>().OfType<DeploymentProfileStartup>().Single().StartAsync(CancellationToken.None);

    void Destroy() => _provider.Dispose();
}
