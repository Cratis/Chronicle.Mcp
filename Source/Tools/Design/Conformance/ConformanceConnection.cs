// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;
using Cratis.Chronicle.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Provides lazy access to the audit-only connection and a single admission slot.
/// </summary>
public sealed class ConformanceConnection
{
    /// <summary>
    /// The service key for the dedicated Chronicle connection.
    /// </summary>
    internal const string Key = "screenplay-conformance";

    /// <summary>
    /// The maximum size of a unary response. Registry reads are not paged: allow an average of
    /// 4 KiB of schema and metadata per admitted entry, subject to this hard byte limit.
    /// </summary>
    internal const int MaxReceiveBytes = ConformanceObservations.MaximumEntries * 4 * 1024;

    int _busy;

    /// <summary>
    /// Resolves the dedicated connection only when the audit needs to read.
    /// </summary>
    /// <param name="services">The deployment's service provider.</param>
    /// <param name="cancellationToken">The caller and scan deadline token.</param>
    /// <returns>The Chronicle services on the bounded channel.</returns>
    internal static ValueTask<IServices> Resolve(IServiceProvider services, CancellationToken cancellationToken) =>
        services.GetRequiredKeyedService<IGetServicesAsynchronously>(Key).GetServices(cancellationToken);

    /// <summary>
    /// Attempts to acquire the single audit slot without waiting.
    /// </summary>
    /// <returns>Whether the slot was acquired.</returns>
    internal bool TryEnter() => Interlocked.CompareExchange(ref _busy, 1, 0) == 0;

    /// <summary>
    /// Releases the audit slot.
    /// </summary>
    internal void Exit() => Volatile.Write(ref _busy, 0);
}
