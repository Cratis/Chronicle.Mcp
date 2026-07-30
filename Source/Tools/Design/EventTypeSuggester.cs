// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Finds gaps in a system's deduced entity model and turns each into a grounded event type
/// suggestion. Every rule is deterministic and explains itself: an entity without a beginning,
/// a life without an end, or a fact set once and never changeable again.
/// </summary>
public static class EventTypeSuggester
{
    /// <summary>
    /// Suggests event types to introduce next for the given entities.
    /// </summary>
    /// <param name="entities">The entities deduced from the store's event types.</param>
    /// <returns>Suggestions grouped by entity, each with the gap it closes.</returns>
    public static IReadOnlyList<EventTypeSuggestion> Suggest(IEnumerable<SystemEntity> entities) =>
        entities.SelectMany(SuggestForEntity).ToList();

    static IEnumerable<EventTypeSuggestion> SuggestForEntity(SystemEntity entity)
    {
        var creations = entity.Events.Where(entityEvent => entityEvent.Stage == LifecycleStage.Creation).ToList();
        var hasTermination = entity.Events.Any(entityEvent => entityEvent.Stage == LifecycleStage.Termination);

        if (creations.Count == 0)
        {
            var rationale =
                $"The {entity.Name} lifecycle starts implicitly — its first recorded fact is '{entity.Events[0].EventType}'. " +
                "An explicit creation event makes the beginning of the lifecycle unambiguous and gives projections a reliable initialization point.";
            yield return new EventTypeSuggestion($"{entity.Name}Created", entity.Name, LifecycleStage.Creation, rationale, []);
        }

        if (!hasTermination)
        {
            var rationale =
                $"The {entity.Name} lifecycle has a beginning but no end — nothing ever removes, closes, or archives a {entity.Name}. " +
                "A termination event lets read models clear state (e.g. [RemovedWith]) and gives compliance and cleanup a fact to act on.";
            yield return new EventTypeSuggestion($"{entity.Name}Removed", entity.Name, LifecycleStage.Termination, rationale, []);
        }

        foreach (var suggestion in SuggestChangeEvents(entity, creations))
        {
            yield return suggestion;
        }
    }

    static IEnumerable<EventTypeSuggestion> SuggestChangeEvents(SystemEntity entity, IReadOnlyList<SystemEntityEvent> creations)
    {
        var laterPropertyNames = entity.Events
            .Where(entityEvent => entityEvent.Stage != LifecycleStage.Creation)
            .SelectMany(entityEvent => entityEvent.Properties)
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var creation in creations)
        {
            foreach (var property in creation.Properties)
            {
                if (property.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || laterPropertyNames.Contains(property.Name))
                {
                    continue;
                }

                var rationale =
                    $"'{property.Name}' is set when the {entity.Name} comes to life ('{creation.EventType}') and no later event ever touches it. " +
                    $"If the business allows a {entity.Name}'s {property.Name} to change, that change needs its own event.";
                yield return new EventTypeSuggestion($"{entity.Name}{Capitalize(property.Name)}Changed", entity.Name, LifecycleStage.Mutation, rationale, [property]);
            }
        }
    }

    static string Capitalize(string value) =>
        value.Length == 0 ? value : string.Concat(char.ToUpperInvariant(value[0]).ToString(), value[1..]);
}
