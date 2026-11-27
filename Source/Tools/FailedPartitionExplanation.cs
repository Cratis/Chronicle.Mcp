// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A read-only explanation of one failed observer partition in a selected scope.
/// </summary>
/// <param name="Status">The outcome of the explanation.</param>
/// <param name="FailedPartitionId">The requested failure identifier.</param>
/// <param name="EventStore">The displayed event store.</param>
/// <param name="Namespace">The displayed namespace.</param>
/// <param name="EventSequenceId">The displayed requested sequence.</param>
/// <param name="Context">The bounded preceding sequence history.</param>
public record FailedPartitionExplanation(
    string Status,
    Guid FailedPartitionId,
    string EventStore,
    string Namespace,
    string EventSequenceId,
    FailedPartitionContext Context)
{
    /// <summary>
    /// Gets the displayed sequence reported by observer metadata, if found.
    /// </summary>
    public string? ObservedEventSequenceId { get; init; }

    /// <summary>
    /// Gets the displayed observer identifier, if found.
    /// </summary>
    public string? ObserverId { get; init; }

    /// <summary>
    /// Gets the displayed partition, if found.
    /// </summary>
    public string? Partition { get; init; }

    /// <summary>
    /// Gets whether the partition failure is resolved, if known.
    /// </summary>
    public bool? IsResolved { get; init; }

    /// <summary>
    /// Gets whether the partition is quarantined, if known.
    /// </summary>
    public bool? IsQuarantined { get; init; }

    /// <summary>
    /// Gets the latest recorded attempt, if present.
    /// </summary>
    public FailedPartitionAttemptExplanation? Attempt { get; init; }

    /// <summary>
    /// Gets whether the credential screen withheld any retained attempt diagnostics.
    /// </summary>
    public bool DiagnosticsRedacted { get; init; }

    /// <summary>
    /// Gets the exact failed event, when established by the read.
    /// </summary>
    public FailedPartitionEventExplanation? FailedEvent { get; init; }

    /// <summary>
    /// Gets whether any displayed scope or failure identifier was shortened.
    /// </summary>
    public bool IdentifiersTruncated { get; init; }
}
