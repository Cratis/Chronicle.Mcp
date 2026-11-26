// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A fixed output window of verified observer associations.
/// </summary>
/// <typeparam name="TRow">The projected evidence type.</typeparam>
/// <param name="Status">Whether evidence could be obtained.</param>
/// <param name="Reason">A fixed reason for the status, when applicable.</param>
/// <param name="Limit">The fixed output limit.</param>
/// <param name="Rows">The retained rows.</param>
/// <param name="Truncated">Whether eligible rows were omitted by the limit.</param>
public sealed record ObserverFailureWindow<TRow>(string Status, string? Reason, int Limit, IReadOnlyList<TRow> Rows, bool Truncated);
