// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A concise description of an appended event.
/// </summary>
/// <param name="SequenceNumber">The sequence number of the event within the sequence.</param>
/// <param name="EventType">The event type id.</param>
/// <param name="Generation">The event type generation.</param>
/// <param name="EventSourceId">The event source id the event belongs to.</param>
/// <param name="Occurred">When the event occurred.</param>
/// <param name="CorrelationId">The correlation id of the event.</param>
/// <param name="Content">The event content, as JSON.</param>
public record EventDescriptor(
    ulong SequenceNumber,
    string EventType,
    uint Generation,
    string EventSourceId,
    DateTimeOffset? Occurred,
    Guid CorrelationId,
    JsonElement? Content);
