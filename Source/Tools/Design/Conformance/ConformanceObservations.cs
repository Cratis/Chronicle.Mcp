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

/// <summary>
/// Reads the registry, namespace subscriptions, and allowlisted append metadata.
/// </summary>
/// <param name="services">The bounded Chronicle services.</param>
/// <param name="scope">The exact target and audit bounds.</param>
/// <param name="context">The call deadline and cancellation context.</param>
internal sealed class ConformanceObservations(IServices services, ConformanceScope scope, CallContext context)
{
    const int MaximumEntries = 4096;
    const ulong FirstSentinel = ulong.MaxValue - 2;

    /// <summary>
    /// Admits an upstream identifier under the audit's length and character limits.
    /// </summary>
    /// <param name="value">The upstream identifier.</param>
    /// <returns>The same identifier if valid.</returns>
    /// <exception cref="ConformanceReadException">The identifier is outside the audit's limits.</exception>
    internal static string Identity(string value) => ConformanceScope.Identifier(value) ? value : throw new ConformanceReadException(ConformanceReadException.Failed);

    /// <summary>
    /// Checks that all three parts of the target exist with exact casing.
    /// </summary>
    /// <returns>A task that completes when the target is confirmed.</returns>
    internal async Task CheckTarget()
    {
        var stores = Data(await services.EventStores.AllEventStores(context));
        Require(stores, store => store.Name, scope.EventStore);
        var namespaces = Data(await services.Namespaces.AllNamespaces(new() { EventStore = scope.EventStore }, context));
        Require(namespaces, item => item.Name, scope.Namespace);
        var sequences = Data(await services.Sequences.AllEventSequences(new() { EventStore = scope.EventStore, Namespace = scope.Namespace }, context));
        Require(sequences, sequence => sequence.Id, scope.Sequence);
    }

    /// <summary>
    /// Captures the current tail of the exact sequence.
    /// </summary>
    /// <returns>The tail, or no position for an empty sequence.</returns>
    internal async Task<long?> Tail()
    {
        var request = new TailSequenceNumberRequest
        {
            EventStore = scope.EventStore,
            Namespace = scope.Namespace,
            EventSequenceId = scope.Sequence
        };
        var result = Data(await services.Sequences.TailSequenceNumber(request, context));
        return result.SequenceNumber >= FirstSentinel ? null : Position(result.SequenceNumber);
    }

    /// <summary>
    /// Reads registered event types and expands their known generations.
    /// </summary>
    /// <returns>The distinct registered identities and generations.</returns>
    /// <exception cref="ConformanceReadException">The registry exceeds the audit's entry limit.</exception>
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

    /// <summary>
    /// Reads subscriptions from actual namespace observer registrations.
    /// </summary>
    /// <returns>The comparison-safe namespace consumers.</returns>
    /// <exception cref="ConformanceReadException">The subscriptions exceed the audit's entry limit.</exception>
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

    /// <summary>
    /// Requests one page of positions without reading event bodies.
    /// </summary>
    /// <param name="from">The first requested position.</param>
    /// <param name="count">The number of positions to request.</param>
    /// <returns>Allowlisted facts about the returned positions.</returns>
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

    static long Position(ulong value) => value <= long.MaxValue ? (long)value : throw new ConformanceReadException(ConformanceReadException.Failed);

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
