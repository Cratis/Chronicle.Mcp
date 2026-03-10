// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Api;

/// <summary>
/// Represents information about an observer in Chronicle.
/// </summary>
/// <param name="Id">The unique identifier of the observer.</param>
/// <param name="EventSequenceId">The event sequence the observer is observing.</param>
/// <param name="Type">The type of observer (0=Unknown, 1=Reactor, 2=Projection, 3=Reducer, 4=External).</param>
/// <param name="EventTypes">The event types the observer is subscribed to.</param>
/// <param name="NextEventSequenceNumber">The next event sequence number to process.</param>
/// <param name="LastHandledEventSequenceNumber">The last handled event sequence number.</param>
/// <param name="RunningState">The running state of the observer (0=Unknown, 1=Active, 2=Suspended, 3=Replaying, 4=Disconnected).</param>
public record ObserverInformation(
    string Id,
    string EventSequenceId,
    int Type,
    IEnumerable<EventType> EventTypes,
    ulong NextEventSequenceNumber,
    ulong LastHandledEventSequenceNumber,
    int RunningState)
{
    static readonly string[] _observerTypes = ["Unknown", "Reactor", "Projection", "Reducer", "External"];
    static readonly string[] _runningStates = ["Unknown", "Active", "Suspended", "Replaying", "Disconnected"];

    /// <summary>
    /// Gets the human-readable name of the observer type.
    /// </summary>
    public string TypeName => Type >= 0 && Type < _observerTypes.Length ? _observerTypes[Type] : "Unknown";

    /// <summary>
    /// Gets the human-readable name of the running state.
    /// </summary>
    public string RunningStateName => RunningState >= 0 && RunningState < _runningStates.Length ? _runningStates[RunningState] : "Unknown";
}
