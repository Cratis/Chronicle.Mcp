// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Detailed information about an observer.
/// </summary>
/// <param name="Id">The observer identifier.</param>
/// <param name="EventSequenceId">The event sequence the observer operates on.</param>
/// <param name="Type">The observer type (e.g. Reactor, Reducer, Projection).</param>
/// <param name="Owner">The owner of the observer (e.g. Client or Chronicle).</param>
/// <param name="RunningState">The current running state of the observer.</param>
/// <param name="IsQuarantined">Whether the observer is quarantined.</param>
/// <param name="NextEventSequenceNumber">The next event sequence number to be handled, or null when none.</param>
/// <param name="LastHandledEventSequenceNumber">The last handled event sequence number, or null when none.</param>
/// <param name="IsSubscribed">Whether the observer currently has a subscription.</param>
/// <param name="EventTypes">The event types the observer reacts to, formatted as id+generation.</param>
public record ObserverDetails(
    string Id,
    string EventSequenceId,
    string Type,
    string Owner,
    string RunningState,
    bool IsQuarantined,
    ulong? NextEventSequenceNumber,
    ulong? LastHandledEventSequenceNumber,
    bool IsSubscribed,
    IReadOnlyList<string> EventTypes);
