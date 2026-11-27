// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Selects the attempt to report across failure diagnostics.
/// </summary>
internal static class FailedPartitionAttempts
{
    /// <summary>
    /// Selects the last recorded attempt at the latest occurrence time.
    /// </summary>
    /// <param name="failure">The failure containing the attempts.</param>
    /// <returns>The latest attempt, or null when none were recorded.</returns>
    internal static FailedPartitionAttempt? LatestAttempt(FailedPartition failure) =>
        (failure.Attempts ?? []).OrderBy(attempt => (DateTimeOffset?)attempt.Occurred).LastOrDefault();
}
