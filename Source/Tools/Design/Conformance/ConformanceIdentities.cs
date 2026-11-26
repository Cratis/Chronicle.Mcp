// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.Sequences;
using Cratis.Screenplay.Observations;
using Cratis.Screenplay.Semantics;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Maps Chronicle producer and consumer identities to Screenplay observations.
/// </summary>
internal static class ConformanceIdentities
{
    static readonly char[] _reactorSeparators = ['.', '+'];

    /// <summary>
    /// Finds the last recognized producer in an append's causation chain.
    /// </summary>
    /// <param name="chain">The recorded causation chain.</param>
    /// <returns>The attributable producer, if present.</returns>
    internal static ObservedProducer? Producer(IEnumerable<Causation>? chain) =>
        (chain ?? []).Select(Recognize).LastOrDefault(producer => producer is not null);

    /// <summary>
    /// Projects the observer identity and its subscription into comparison evidence.
    /// </summary>
    /// <param name="observer">The namespace observer descriptor.</param>
    /// <param name="subscriptions">The observer's event types and generations.</param>
    /// <returns>The comparison-safe consumer.</returns>
    internal static ObservedConsumer Consumer(ObserverInformation observer, ImmutableArray<ObservedEventType> subscriptions)
    {
        var kind = observer.Type switch
        {
            ObserverType.Reactor => ObservedConsumerKind.Reaction,
            ObserverType.Projection => ObservedConsumerKind.Projection,
            ObserverType.Reducer => ObservedConsumerKind.Reducer,
            _ => ObservedConsumerKind.Other
        };
        var owner = observer.Owner switch
        {
            ObserverOwner.Client => ObservedConsumerOwner.Application,
            ObserverOwner.Kernel => ObservedConsumerOwner.Platform,
            _ => ObservedConsumerOwner.Unknown
        };
        return new(
            ConformanceObservations.Identity(observer.Id),
            kind,
            owner,
            ConformanceObservations.Identity(observer.EventSequenceId),
            subscriptions,
            Progress(observer.NextEventSequenceNumber),
            Progress(observer.LastHandledEventSequenceNumber));
    }

    /// <summary>
    /// Adds uniquely matched application reactors to the model's default identity map.
    /// </summary>
    /// <param name="model">The compiled Screenplay model.</param>
    /// <param name="consumers">The observed namespace consumers.</param>
    /// <returns>The identities used for comparison.</returns>
    internal static ObservationIdentityMap Map(ExecutableSemanticModel model, IEnumerable<ObservedConsumer> consumers)
    {
        var map = ObservationIdentityMap.FromModel(model);
        var reactors = consumers.Where(consumer => consumer.Kind == ObservedConsumerKind.Reaction && consumer.Owner == ObservedConsumerOwner.Application)
            .Select(consumer => consumer.Identifier).Distinct(StringComparer.Ordinal).ToArray();
        var reactions = model.Application.Modules.SelectMany(module => module.Features.SelectMany(Reactions));
        foreach (var group in reactions.GroupBy(reaction => reaction.Name, StringComparer.Ordinal).Where(group => group.Count() == 1))
        {
            var matches = reactors.Where(reactor => reactor == group.Key || reactor[(reactor.LastIndexOfAny(_reactorSeparators) + 1)..] == group.Key).Take(2).ToArray();
            if (matches is [var reactor]) map = map.With(group.Single().Id, producer: new(ObservedProducerKind.Reaction, reactor), consumerIdentifier: reactor);
        }

        return map;
    }

    static ObservedProducer? Recognize(Causation causation)
    {
        var (kind, key) = causation.Type switch
        {
            "Command" => (ObservedProducerKind.Command, "commandType"),
            "Client Reactor" => (ObservedProducerKind.Reaction, "ReactorId"),
            _ => (default, null)
        };
        if (key is null || causation.Properties is null) return null;
        return causation.Properties.TryGetValue(key, out var name) && !string.IsNullOrWhiteSpace(name)
            ? new(kind, ConformanceObservations.Identity(name))
            : null;
    }

    static long? Progress(ulong position) => position <= long.MaxValue ? (long)position : null;

    static IEnumerable<SemanticReaction> Reactions(SemanticFeature feature) =>
        feature.Slices.SelectMany(slice => slice.Reactions).Concat(feature.Features.SelectMany(Reactions));
}
