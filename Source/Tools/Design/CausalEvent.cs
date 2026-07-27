// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A single event in a causal trace, carrying the metadata needed to explain why it happened.
/// </summary>
/// <param name="SequenceNumber">The sequence number of the event.</param>
/// <param name="EventType">The event type id.</param>
/// <param name="Generation">The event type generation.</param>
/// <param name="Occurred">When the event occurred.</param>
/// <param name="CorrelationId">The correlation id grouping this event with the wider flow it belongs to.</param>
/// <param name="CausedBy">The identity that caused the event, when known.</param>
/// <param name="Causation">The chain of causes recorded for this event.</param>
/// <param name="Content">The event payload as JSON.</param>
public record CausalEvent(
    ulong SequenceNumber,
    string EventType,
    uint Generation,
    DateTimeOffset? Occurred,
    Guid CorrelationId,
    CausedByDescriptor? CausedBy,
    IReadOnlyList<CausationEntry> Causation,
    JsonElement? Content);
