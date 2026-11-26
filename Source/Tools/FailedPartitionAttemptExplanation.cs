// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Bounded diagnostics for the latest recorded failure attempt.
/// </summary>
/// <param name="Kind">The classification of the failure.</param>
/// <param name="Occurred">The time of the attempt, if known.</param>
/// <param name="SequenceNumber">The position recorded on the attempt.</param>
/// <param name="Messages">The retained diagnostic messages.</param>
/// <param name="StackTrace">The retained portion of the stack trace, if present.</param>
/// <param name="MessagesTruncated">Whether further messages were omitted.</param>
/// <param name="MessageTextTruncated">Whether any retained message was shortened.</param>
/// <param name="StackTraceTruncated">Whether the stack trace was shortened.</param>
public record FailedPartitionAttemptExplanation(
    string Kind,
    DateTimeOffset? Occurred,
    ulong SequenceNumber,
    IReadOnlyList<string> Messages,
    string? StackTrace,
    bool MessagesTruncated,
    bool MessageTextTruncated,
    bool StackTraceTruncated);
