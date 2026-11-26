// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Screenplay.Observations;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

internal sealed record ConformanceSnapshot(ObservationSnapshot Evidence, long? NextFrom, long? NextTo)
{
    const int PageSize = 500;
    const int ScanLimit = 4 * PageSize;

    internal static async Task<ConformanceSnapshot> Capture(ConformanceObservations reads, ConformanceScope scope, TimeProvider clock, CancellationToken cancellationToken)
    {
        await reads.CheckTarget();
        var tail = await reads.Tail();
        var capturedAt = clock.GetUtcNow();
        var registered = await reads.Registrations();
        var consumers = await reads.Consumers();
        var end = Math.Min(scope.To, tail ?? -1);
        var count = end < scope.From ? 0 : (int)Math.Min(ScanLimit - 1, end - scope.From) + 1;
        var records = await ReadPositions(reads, scope, count, cancellationToken);
        var last = count == 0 ? (long?)null : scope.From + (count - 1);
        var coverage = new ObservedSequenceCoverage(scope.Sequence, tail is null || scope.From == 0,
            count == 0 ? null : scope.From, last, tail, [.. Gaps(records, scope.From)], scope.OccurredFrom, scope.OccurredTo);
        var groups = GroupAppends(registered, records).ToArray();
        var snapshot = ObservationSnapshot.Create(scope.EventStore, scope.Namespace, coverage, registered,
            groups.GroupBy(group => group.Key.Type).Select(group => new ObservedAppend(group.Key, group.Sum(items => items.LongCount()))),
            groups.Where(group => group.Key.Producer is not null).Select(group => new ObservedEdge(group.Key.Producer!, group.Key.Type, group.LongCount())),
            groups.Where(group => group.Key.Producer is null).Select(group => new UnattributedAppend(group.Key.Type, group.LongCount())), consumers, capturedAt);
        return last is { } limit && limit < end ? new(snapshot, limit + 1, end) : new(snapshot, null, null);
    }

    static async Task<ConformanceAppend?[]> ReadPositions(ConformanceObservations reads, ConformanceScope scope, int count, CancellationToken cancellationToken)
    {
        var positions = new ConformanceAppend?[count];
        for (var offset = 0; offset < count; offset += PageSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var start = scope.From + offset;
            var size = Math.Min(PageSize, count - offset);
            var page = await reads.Metadata(start, size);
            RecordPage(positions, page, start, offset, size);
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
