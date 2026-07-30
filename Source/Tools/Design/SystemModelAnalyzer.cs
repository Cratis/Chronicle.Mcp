// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Deduces a structural model of a system from its registered event types — clustering events into
/// the entities they describe and placing each event in that entity's lifecycle. This is the shared
/// deduction underneath the system description and evolution suggestion tools.
/// </summary>
public static class SystemModelAnalyzer
{
    /// <summary>
    /// Builds the entity model for a set of event types and their schema properties.
    /// </summary>
    /// <param name="eventTypes">The event type ids with the properties parsed from their schemas.</param>
    /// <returns>The deduced entities, alphabetically ordered, each with its events ordered by lifecycle stage.</returns>
    /// <remarks>
    /// Property-scoped clusters coalesce into their parent entity when one exists: with both
    /// AuthorRegistered and AuthorNameChanged present, "AuthorName" folds into "Author" so the
    /// entity's full lifecycle sits together.
    /// </remarks>
    public static IReadOnlyList<SystemEntity> BuildEntities(IReadOnlyDictionary<string, IReadOnlyList<EventSchemaProperty>> eventTypes)
    {
        var clusters = eventTypes
            .Select(eventType => new
            {
                Entity = EventNameMorphology.InferEntity(eventType.Key),
                Event = new SystemEntityEvent(eventType.Key, EventNameMorphology.Classify(eventType.Key), eventType.Value)
            })
            .GroupBy(entry => entry.Entity, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Select(entry => entry.Event).ToList(), StringComparer.OrdinalIgnoreCase);

        return clusters
            .GroupBy(cluster => Canonical(cluster.Key, clusters), StringComparer.OrdinalIgnoreCase)
            .Select(group => new SystemEntity(
                group.Key,
                group
                    .SelectMany(cluster => cluster.Value)
                    .OrderBy(entityEvent => StageOrder(entityEvent.Stage))
                    .ThenBy(entityEvent => entityEvent.EventType, StringComparer.OrdinalIgnoreCase)
                    .ToList()))
            .OrderBy(entity => entity.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    static string Canonical(string entity, IReadOnlyDictionary<string, List<SystemEntityEvent>> clusters)
    {
        var current = entity;
        var parent = LongestKnownPrefix(current, clusters);
        while (parent is not null)
        {
            current = parent;
            parent = LongestKnownPrefix(current, clusters);
        }

        return current;
    }

    static string? LongestKnownPrefix(string entity, IReadOnlyDictionary<string, List<SystemEntityEvent>> clusters)
    {
        var words = EventNameMorphology.SplitWords(entity);
        for (var length = words.Count - 1; length >= 1; length--)
        {
            var candidate = string.Concat(words.Take(length));
            if (clusters.ContainsKey(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    static int StageOrder(LifecycleStage stage) => stage switch
    {
        LifecycleStage.Creation => 0,
        LifecycleStage.Mutation => 1,
        LifecycleStage.Activity => 2,
        LifecycleStage.Correction => 3,
        LifecycleStage.Termination => 4,
        _ => 5
    };
}
