// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.Sequences;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Projects only the permitted fields of a failed partition explanation within its output budgets.
/// </summary>
internal static class FailedPartitionExplanationFormatting
{
    const int MaxIdentifierLength = 512;
    const int MaxMessages = 10;
    const int MaxMessageLength = 2048;
    const int MaxStackLength = 8192;
    const int MaxContentLength = 4096;

    /// <summary>
    /// Prepares an empty explanation for the selected scope and numerical limits.
    /// </summary>
    /// <param name="id">The requested failure ID.</param>
    /// <param name="scope">The resolved store and namespace.</param>
    /// <param name="sequenceId">The sequence selected by the caller.</param>
    /// <param name="precedingCount">The preceding event count requested by the caller.</param>
    /// <returns>The scoped explanation before a matching failure is found.</returns>
    internal static FailedPartitionExplanation ForScope(Guid id, GetFailedPartitionsRequest scope, string sequenceId, int precedingCount)
    {
        var limit = Math.Clamp(precedingCount, 0, 50);
        var context = new FailedPartitionContext(limit == 0 ? "not_requested" : "unavailable", precedingCount, limit);
        return new("not_found", id, Display(scope.EventStore), Display(scope.Namespace), Display(sequenceId), context)
        {
            IdentifiersTruncated = IdentifiersTooLong(scope.EventStore, scope.Namespace, sequenceId)
        };
    }

    /// <summary>
    /// Adds bounded failure metadata and latest-attempt diagnostics to the explanation.
    /// </summary>
    /// <param name="result">The explanation to enrich.</param>
    /// <param name="failure">The matched failure.</param>
    /// <param name="attempt">The latest attempt, if any.</param>
    /// <returns>The explanation with failure metadata.</returns>
    internal static FailedPartitionExplanation WithFailure(
        FailedPartitionExplanation result,
        FailedPartition failure,
        FailedPartitionAttempt? attempt) => result with
    {
        ObserverId = Display(failure.ObserverId),
        Partition = Display(failure.Partition),
        IsResolved = failure.IsResolved,
        IsQuarantined = failure.IsQuarantined,
        Attempt = attempt is null ? null : DescribeAttempt(attempt),
        IdentifiersTruncated = result.IdentifiersTruncated || IdentifiersTooLong(failure.ObserverId, failure.Partition)
    };

    /// <summary>
    /// Adds the sequence reported by the observer metadata.
    /// </summary>
    /// <param name="result">The explanation to enrich.</param>
    /// <param name="sequenceId">The observer's sequence ID.</param>
    /// <returns>The explanation with the bounded observed sequence ID.</returns>
    internal static FailedPartitionExplanation WithObservedSequence(FailedPartitionExplanation result, string sequenceId) => result with
    {
        ObservedEventSequenceId = Display(sequenceId),
        IdentifiersTruncated = result.IdentifiersTruncated || IdentifiersTooLong(sequenceId)
    };

    /// <summary>
    /// Validates and projects the exact event and nearest preceding events from one page.
    /// </summary>
    /// <param name="result">The explanation to enrich.</param>
    /// <param name="page">The backward page returned by Chronicle.</param>
    /// <param name="position">The recorded failed position.</param>
    /// <returns>The explanation with event evidence from the page.</returns>
    /// <exception cref="InvalidOperationException">The page exceeds the requested count or contains invalid positions.</exception>
    internal static FailedPartitionExplanation WithPage(FailedPartitionExplanation result, EventSequencePageResponse page, ulong position)
    {
        var limit = result.Context.Limit;
        var rows = page.Events.Take(limit + 2).OrderByDescending(row => row.Context.SequenceNumber).ToArray();
        if (rows.Length > limit + 1 || HasInvalidPositions(rows, position))
        {
            throw new InvalidOperationException("Chronicle returned an invalid event page.");
        }

        var exact = rows.FirstOrDefault(row => row.Context.SequenceNumber == position);
        var older = rows.Where(row => row.Context.SequenceNumber < position).ToArray();
        return result with
        {
            Status = exact is null ? "missing_event" : "explained",
            FailedEvent = exact is null ? null : DescribeEvent(exact),
            Context = result.Context with
            {
                Status = limit == 0 ? "not_requested" : "available",
                Events = older.Take(limit).Reverse().Select(DescribeEvent).ToArray(),
                Truncated = page.Next.HasValue || older.Length > limit
            }
        };
    }

    static bool HasInvalidPositions(AppendedEventResponse[] rows, ulong position) =>
        rows.Any(row => row.Context.SequenceNumber > position) || rows.DistinctBy(row => row.Context.SequenceNumber).Count() != rows.Length;

    static FailedPartitionEventExplanation DescribeEvent(AppendedEventResponse row)
    {
        var header = row.Context;
        var contentStatus = GetContentStatus(row);
        return new(
            header.SequenceNumber,
            Display(header.EventType.Id),
            header.EventType.Generation,
            Display(header.EventSourceId),
            header.Occurred,
            header.CorrelationId,
            contentStatus == "present" ? row.Content : null,
            contentStatus,
            IdentifiersTooLong(header.EventType.Id, header.EventSourceId));
    }

    static FailedPartitionAttemptExplanation DescribeAttempt(FailedPartitionAttempt attempt)
    {
        var messages = (attempt.Messages ?? []).Take(MaxMessages + 1).ToArray();
        var retained = messages.Take(MaxMessages).ToArray();
        return new(
            attempt.Kind.ToString(),
            attempt.Occurred,
            attempt.SequenceNumber,
            retained.Select(message => Shorten(message, MaxMessageLength)).ToArray(),
            attempt.StackTrace is { } stack ? Shorten(stack, MaxStackLength) : null,
            messages.Length > MaxMessages,
            retained.Any(message => message.Length > MaxMessageLength),
            attempt.StackTrace?.Length > MaxStackLength);
    }

    static string GetContentStatus(AppendedEventResponse row)
    {
        if (row.ErasedProperties?.Any() is true) return "redacted";
        if (string.IsNullOrEmpty(row.Content)) return "unavailable";
        return row.Content.Length > MaxContentLength ? "budget_exceeded" : "present";
    }

    static bool IdentifiersTooLong(params string[] values) => values.Any(value => value.Length > MaxIdentifierLength);

    static string Display(string value) => Shorten(value, MaxIdentifierLength);

    static string Shorten(string value, int limit)
    {
        if (value.Length <= limit) return value;
        var end = char.IsHighSurrogate(value[limit - 1]) ? limit - 1 : limit;
        return value[..end];
    }
}
