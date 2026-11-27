// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Freezes the catalog before hosted services start accepting calls.
/// Unavailable catalogs still allow the host and inline audits to run.
/// </summary>
/// <param name="services">The host provider that owns the snapshot.</param>
internal sealed class CatalogStartup(IServiceProvider services) : IHostedLifecycleService
{
    /// <inheritdoc/>
    public Task StartingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = services.GetRequiredService<ConformanceModelCatalog>();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>Registers the owned snapshot and its eager host initialization.</summary>
    /// <param name="services">The host services that will own the snapshot.</param>
    internal static void Register(IServiceCollection services)
    {
        services.AddOptions();
        services.TryAddSingleton<ConformanceModelCatalog>();
        services.AddHostedService<CatalogStartup>();
    }
}
