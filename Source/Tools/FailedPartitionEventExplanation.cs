// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// An event's minimal headers and optional compliance-filtered current content.
/// </summary>
/// <param name="SequenceNumber">The position in the verified sequence.</param>
/// <param name="EventTypeId">The displayed event type identifier.</param>
/// <param name="Generation">The event type generation.</param>
/// <param name="EventSourceId">The displayed event source identifier.</param>
/// <param name="Occurred">The event occurrence time, if known.</param>
/// <param name="CorrelationId">The correlation identifier.</param>
/// <param name="Content">Current content as returned by Chronicle, if included.</param>
/// <param name="ContentStatus">Whether content is present or why it was omitted.</param>
/// <param name="HeadersTruncated">Whether any displayed event header was shortened.</param>
public record FailedPartitionEventExplanation(
    ulong SequenceNumber,
    string EventTypeId,
    uint Generation,
    string EventSourceId,
    DateTimeOffset? Occurred,
    Guid CorrelationId,
    string? Content,
    string ContentStatus,
    bool HeadersTruncated);
