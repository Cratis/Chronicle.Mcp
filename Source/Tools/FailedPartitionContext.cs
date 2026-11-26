// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A bounded window of preceding sequence positions, not a causal or partition history.
/// </summary>
/// <param name="Status">Whether preceding history was inspected, unavailable or not requested.</param>
/// <param name="RequestedLimit">The preceding event count requested by the caller.</param>
/// <param name="Limit">The effective preceding event limit.</param>
public record FailedPartitionContext(string Status, int RequestedLimit, int Limit)
{
    /// <summary>
    /// Gets whether the requested limit was clamped.
    /// </summary>
    public bool LimitApplied => RequestedLimit != Limit;

    /// <summary>
    /// Gets the preceding events, ordered by increasing position.
    /// </summary>
    public IReadOnlyList<FailedPartitionEventExplanation> Events { get; init; } = [];

    /// <summary>
    /// Gets whether older events were omitted from the window.
    /// </summary>
    public bool Truncated { get; init; }
}
