// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Contracts.Sequences;
using Cratis.Chronicle.Mcp.Authorization;
using Cratis.Screenplay.Observations;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

internal sealed class ConformanceObservations(IServices services, ConformanceScope scope, CallContext context)
{
    const int MaximumEntries = 4096;
    const ulong FirstSentinel = ulong.MaxValue - 2;

    internal async Task CheckTarget()
    {
        var stores = Data(await services.EventStores.AllEventStores(context));
        Require(stores, store => store.Name, scope.EventStore);
        var namespaces = Data(await services.Namespaces.AllNamespaces(new() { EventStore = scope.EventStore }, context));
        Require(namespaces, item => item.Name, scope.Namespace);
        var sequences = Data(await services.Sequences.AllEventSequences(new() { EventStore = scope.EventStore, Namespace = scope.Namespace }, context));
        Require(sequences, sequence => sequence.Id, scope.Sequence);
    }

    internal async Task<long?> Tail()
    {
        var result = Data(await services.Sequences.TailSequenceNumber(new()
        {
            EventStore = scope.EventStore,
            Namespace = scope.Namespace,
            EventSequenceId = scope.Sequence
        }, context));
        return result.SequenceNumber >= FirstSentinel ? null : Position(result.SequenceNumber);
    }

    internal async Task<ObservedEventType[]> Registrations()
    {
        var entries = Bounded(Data(await services.EventTypes.AllEventTypes(new() { EventStore = scope.EventStore }, context)));
        var types = new HashSet<ObservedEventType>();
        var total = 0u;
        foreach (var entry in entries)
        {
            var id = Identity(entry.Type.Id);
            var count = Math.Max(1u, entry.Type.Generation);
            if (count > MaximumEntries - total) throw new ConformanceReadException(ConformanceReadException.Limit);
            total += count;
            for (var generation = 1u; generation <= count; generation++) types.Add(new(id, generation));
        }

        return [.. types];
    }

    internal async Task<ObservedConsumer[]> Consumers()
    {
        // Store-wide definitions are intentionally not evidence of a namespace subscription.
        _ = Bounded(await services.Projections.GetAllDefinitions(new() { EventStore = scope.EventStore }, context));
        var observers = Bounded(await services.Observers.GetObservers(new AllObserversRequest { EventStore = scope.EventStore, Namespace = scope.Namespace }, context));
        var consumers = new List<ObservedConsumer>();
        var subscriptions = 0;
        foreach (var observer in observers)
        {
            var types = Bounded(observer.EventTypes ?? []);
            subscriptions += types.Length;
            if (subscriptions > MaximumEntries) throw new ConformanceReadException(ConformanceReadException.Limit);
            consumers.Add(ConformanceIdentities.Consumer(observer, [.. types.Select(type => new ObservedEventType(Identity(type.Id), type.Generation)).Distinct()]));
        }

        return [.. consumers];
    }

    internal async Task<ConformanceAppend[]> Metadata(long from, int count)
    {
        var request = new MetadataAtRequest
        {
            EventStore = scope.EventStore,
            Namespace = scope.Namespace,
            EventSequenceId = scope.Sequence,
            SequenceNumbers = Enumerable.Range(0, count).Select(offset => (ulong)(from + offset)).ToArray()
        };
        var records = Data(await services.Sequences.MetadataAt(request, context));
        return [.. Bounded(records).Select(entry => new ConformanceAppend(
            Position(entry.SequenceNumber), Identity(entry.EventTypeId), entry.Occurred, ConformanceIdentities.Producer(entry.Causation)))];
    }

    internal static string Identity(string value) => ConformanceScope.Identifier(value) ? value : throw new ConformanceReadException(ConformanceReadException.Failed);

    internal static long Position(ulong value) => value <= long.MaxValue ? (long)value : throw new ConformanceReadException(ConformanceReadException.Failed);

    static T Data<T>(QueryResult<T> result)
    {
        if (!result.IsAuthorized) throw new ConformanceReadException(DenialCodes.ChronicleDenied);
        return result.IsSuccess && result.Data is not null ? result.Data : throw new ConformanceReadException(ConformanceReadException.Failed);
    }

    static T[] Bounded<T>(IEnumerable<T> values)
    {
        var entries = values.Take(MaximumEntries + 1).ToArray();
        return entries.Length <= MaximumEntries ? entries : throw new ConformanceReadException(ConformanceReadException.Limit);
    }

    static void Require<T>(IEnumerable<T> entries, Func<T, string> name, string expected)
    {
        if (!Bounded(entries).Any(entry => string.Equals(name(entry), expected, StringComparison.Ordinal)))
        {
            throw new ConformanceReadException(ConformanceReadException.MissingTarget);
        }
    }
}
