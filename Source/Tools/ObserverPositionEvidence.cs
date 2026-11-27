// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Facts from one observer metadata row, not measurements of its progress.
/// </summary>
/// <param name="EventSequenceId">The observer's reported sequence identifier.</param>
/// <param name="Type">The reported observer type.</param>
/// <param name="Owner">The reported observer owner.</param>
/// <param name="RunningState">The reported running state.</param>
/// <param name="RunningStateChangedAt">When the reported running state last changed, if supplied.</param>
/// <param name="IsReplayable">Whether the observer is marked replayable.</param>
/// <param name="IsQuarantined">Whether the reported state is quarantined, or null for an unknown state.</param>
/// <param name="NextEventSequenceNumber">The next position, if actual.</param>
/// <param name="LastHandledEventSequenceNumber">The last handled position, if actual.</param>
/// <param name="TailEventSequenceNumber">The tail position reported by the observer, if actual.</param>
/// <param name="HandledEventCount">The observer's reported handled event count.</param>
public sealed record ObserverPositionEvidence(
    string EventSequenceId,
    string Type,
    string Owner,
    string RunningState,
    DateTimeOffset? RunningStateChangedAt,
    bool IsReplayable,
    bool? IsQuarantined,
    ulong? NextEventSequenceNumber,
    ulong? LastHandledEventSequenceNumber,
    ulong? TailEventSequenceNumber,
    ulong HandledEventCount);
