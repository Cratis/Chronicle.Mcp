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
    internal const string Key = "screenplay-conformance";
    internal const int MaxReceiveBytes = 64 * 1024;

    int _busy;

    internal static IServices Resolve(IServiceProvider services) =>
        ((IChronicleServicesAccessor)services.GetRequiredKeyedService<IChronicleConnection>(Key)).Services;

    internal bool TryEnter() => Interlocked.CompareExchange(ref _busy, 1, 0) == 0;

    internal void Exit() => Volatile.Write(ref _busy, 0);
}
