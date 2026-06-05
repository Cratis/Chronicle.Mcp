// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A concise description of a failed observer partition.
/// </summary>
/// <param name="Id">The identifier of the failed partition.</param>
/// <param name="ObserverId">The identifier of the observer that failed.</param>
/// <param name="Partition">The partition (event source) that failed.</param>
/// <param name="Attempts">The number of recovery attempts made.</param>
/// <param name="LastFailedSequenceNumber">The sequence number where the most recent failure occurred.</param>
/// <param name="LastFailureReason">The messages from the most recent failure.</param>
/// <param name="LastOccurred">When the most recent failure occurred.</param>
public record FailedPartitionDescriptor(
    Guid Id,
    string ObserverId,
    string Partition,
    int Attempts,
    ulong? LastFailedSequenceNumber,
    string? LastFailureReason,
    DateTimeOffset? LastOccurred);
