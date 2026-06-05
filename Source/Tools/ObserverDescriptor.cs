// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A concise description of an observer.
/// </summary>
/// <param name="Id">The observer identifier.</param>
/// <param name="Type">The observer type (e.g. Reactor, Reducer, Projection).</param>
/// <param name="RunningState">The current running state of the observer.</param>
/// <param name="IsQuarantined">Whether the observer is quarantined.</param>
/// <param name="NextEventSequenceNumber">The next event sequence number to be handled, or null when none.</param>
/// <param name="LastHandledEventSequenceNumber">The last handled event sequence number, or null when none.</param>
/// <param name="IsSubscribed">Whether the observer currently has a subscription.</param>
public record ObserverDescriptor(
    string Id,
    string Type,
    string RunningState,
    bool IsQuarantined,
    ulong? NextEventSequenceNumber,
    ulong? LastHandledEventSequenceNumber,
    bool IsSubscribed);
