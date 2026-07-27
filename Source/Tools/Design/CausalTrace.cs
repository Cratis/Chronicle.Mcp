// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// The ordered causal history of an event source — what happened and why — for turning raw event logs
/// into a plain-language narrative ("git blame for state").
/// </summary>
/// <param name="EventStore">The event store the trace was read from.</param>
/// <param name="Namespace">The namespace the trace was read from.</param>
/// <param name="EventSequenceId">The event sequence the trace was read from.</param>
/// <param name="EventSourceId">The event source the trace is for.</param>
/// <param name="EventCount">The number of events in the trace.</param>
/// <param name="Events">The events in sequence order, each with its correlation and causation metadata.</param>
public record CausalTrace(
    string EventStore,
    string Namespace,
    string EventSequenceId,
    string EventSourceId,
    int EventCount,
    IReadOnlyList<CausalEvent> Events);
