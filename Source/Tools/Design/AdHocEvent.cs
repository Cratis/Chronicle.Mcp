// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// An event prepared for ad-hoc folding — the minimal shape the folder needs.
/// </summary>
/// <param name="EventSourceId">The event source the event belongs to.</param>
/// <param name="EventType">The event type id.</param>
/// <param name="SequenceNumber">The event's position in the sequence.</param>
/// <param name="Occurred">When the event occurred.</param>
/// <param name="Content">The event's JSON content, when parseable.</param>
public record AdHocEvent(
    string EventSourceId,
    string EventType,
    ulong SequenceNumber,
    DateTimeOffset? Occurred,
    JsonElement? Content);
