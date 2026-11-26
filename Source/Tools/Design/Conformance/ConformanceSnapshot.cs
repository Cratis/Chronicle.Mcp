// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Screenplay.Observations;
using Grpc.Core;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Captured evidence together with the remaining positions, if the scan was capped.
/// </summary>
/// <param name="Evidence">The bounded observation snapshot.</param>
/// <param name="NextFrom">The next range's inclusive starting position, if any.</param>
/// <param name="NextTo">The next range's inclusive ending position, if any.</param>
internal sealed record ConformanceSnapshot(ObservationSnapshot Evidence, long? NextFrom, long? NextTo)
{
    const int PageSize = 500;
    const int ScanLimit = 2000;
    const int ReadAttemptLimit = 256;

    /// <summary>
    /// Captures target, registry, subscription, and append evidence for a bounded range.
    /// </summary>
    /// <param name="reads">The Chronicle metadata reader.</param>
    /// <param name="scope">The validated target and bounds.</param>
    /// <param name="clock">The clock used to timestamp evidence.</param>
    /// <param name="cancellationToken">The scan cancellation token.</param>
    /// <returns>The evidence and any remaining range.</returns>
    internal static async Task<ConformanceSnapshot> Capture(ConformanceObservations reads, ConformanceScope scope, TimeProvider clock, CancellationToken cancellationToken)
    {
        await reads.CheckTarget();
        var tail = await reads.Tail();
        var capturedAt = clock.GetUtcNow();
        var registered = await reads.Registrations();
        var consumers = await reads.Consumers();
        var start = scope.Positions.From;
        var end = Math.Min(scope.Positions.To, tail ?? -1);
        var count = end < start ? 0 : (int)Math.Min(ScanLimit - 1, end - start) + 1;
        var records = await ReadPositions(reads, scope, count, cancellationToken);
        var last = count == 0 ? (long?)null : start + (count - 1);
        var coverage = new ObservedSequenceCoverage(
            scope.Sequence,
            tail is null || start == 0,
            count == 0 ? null : start,
            last,
            tail,
            [.. Gaps(records, start)],
            scope.Window.From,
            scope.Window.To);
        var groups = GroupAppends(registered, records).ToArray();
        var snapshot = ObservationSnapshot.Create(
            scope.EventStore,
            scope.Namespace,
            coverage,
            registered,
            groups.GroupBy(group => group.Key.Type).Select(group => new ObservedAppend(group.Key, group.Sum(items => items.LongCount()))),
            groups.Where(group => group.Key.Producer is not null).Select(group => new ObservedEdge(group.Key.Producer!, group.Key.Type, group.LongCount())),
            groups.Where(group => group.Key.Producer is null).Select(group => new UnattributedAppend(group.Key.Type, group.LongCount())),
            consumers,
            capturedAt);
        return last is { } limit && limit < end ? new(snapshot, limit + 1, end) : new(snapshot, null, null);
    }

    static async Task<ConformanceAppend?[]> ReadPositions(ConformanceObservations reads, ConformanceScope scope, int count, CancellationToken cancellationToken)
    {
        var positions = new ConformanceAppend?[count];
        var pageSize = PageSize;
        var offset = 0;
        for (var attempt = 0; offset < count; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (attempt == ReadAttemptLimit) throw new ConformanceReadException(ConformanceReadException.Limit);
            var start = scope.Positions.From + offset;
            var size = Math.Min(pageSize, count - offset);
            try
            {
                var page = await reads.Metadata(start, size);
                RecordPage(positions, page, start, offset, size);
                offset += size;
            }
            catch (RpcException exception) when (exception.StatusCode == StatusCode.ResourceExhausted && size > 1)
            {
                // No positions were examined on failure. Retry the same start with fewer positions.
                pageSize = size / 2;
            }
        }

        return positions.Select(record => record is not null && scope.Includes(record.Occurred) ? record : null).ToArray();
    }

    static void RecordPage(ConformanceAppend?[] positions, ConformanceAppend[] page, long start, int offset, int size)
    {
        foreach (var record in page)
        {
            if (record.Position < start || record.Position - start >= size) throw new ConformanceReadException(ConformanceReadException.Failed);
            var index = offset + (int)(record.Position - start);
            if (positions[index] is { } previous && previous != record) throw new ConformanceReadException(ConformanceReadException.Failed);
            positions[index] = record;
        }
    }

    static IEnumerable<ObservedRange> Gaps(ConformanceAppend?[] records, long first)
    {
        for (var index = 0; index < records.Length; index++)
        {
            if (records[index] is not null) continue;
            var start = index;
            while (index + 1 < records.Length && records[index + 1] is null) index++;
            yield return new(first + start, first + index);
        }
    }

    static IEnumerable<IGrouping<(ObservedEventType Type, ObservedProducer? Producer), ConformanceAppend>> GroupAppends(ObservedEventType[] registered, ConformanceAppend?[] records)
    {
        var generations = registered.GroupBy(type => type.TypeId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Max(type => type.Generation), StringComparer.Ordinal);
        return records.OfType<ConformanceAppend>().GroupBy(record =>
            (new ObservedEventType(record.TypeId, generations.GetValueOrDefault(record.TypeId, 1u)), record.Producer));
    }
}
