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
    /// The maximum size of a received gRPC message.
    /// </summary>
    internal const int MaxReceiveBytes = 64 * 1024;

    int _busy;

    /// <summary>
    /// Resolves the dedicated connection only when the audit needs to read.
    /// </summary>
    /// <param name="services">The deployment's service provider.</param>
    /// <returns>The Chronicle services on the bounded channel.</returns>
    internal static IServices Resolve(IServiceProvider services) =>
        ((IChronicleServicesAccessor)services.GetRequiredKeyedService<IChronicleConnection>(Key)).Services;

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
