// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Freezes the catalog during startup validation, before hosted services start.
/// Unavailable catalogs still allow the host and inline audits to run.
/// </summary>
/// <param name="services">The host provider that owns the snapshot.</param>
internal sealed class CatalogStartup(IServiceProvider services) : IAsyncStartupValidator
{
    /// <inheritdoc/>
    public Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = services.GetRequiredService<ConformanceModelCatalog>();
        return Task.CompletedTask;
    }

    /// <summary>Registers the owned snapshot and its eager host initialization.</summary>
    /// <param name="services">The host services that will own the snapshot.</param>
    internal static void Register(IServiceCollection services)
    {
        services.AddOptions();
        services.TryAddSingleton<ConformanceModelCatalog>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAsyncStartupValidator, CatalogStartup>());
    }
}
