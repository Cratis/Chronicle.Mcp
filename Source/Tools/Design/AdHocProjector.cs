// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Folds a stream of events into per-event-source instances the way a Chronicle projection with
/// AutoMap would: properties merge by name, later events overwrite earlier ones, and removal
/// events drop the instance. This runs entirely in the MCP server against events read from the
/// store — nothing is registered or written.
/// </summary>
public static class AdHocProjector
{
    /// <summary>
    /// Folds events, ordered by sequence number, into the current state per event source.
    /// </summary>
    /// <param name="events">The events to fold.</param>
    /// <param name="removedWithEventTypes">Event type ids that remove an instance (case-insensitive).</param>
    /// <returns>The materialized instances, ordered by event source id.</returns>
    public static IReadOnlyList<AdHocProjectionInstance> Fold(
        IEnumerable<AdHocEvent> events,
        IEnumerable<string>? removedWithEventTypes = null)
    {
        var removedWith = (removedWithEventTypes ?? []).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var instances = new Dictionary<string, InstanceState>(StringComparer.Ordinal);

        foreach (var @event in events.OrderBy(entry => entry.SequenceNumber))
        {
            if (removedWith.Contains(@event.EventType))
            {
                instances.Remove(@event.EventSourceId);
                continue;
            }

            if (!instances.TryGetValue(@event.EventSourceId, out var instance))
            {
                instance = new InstanceState();
                instances[@event.EventSourceId] = instance;
            }

            Merge(instance.Properties, @event.Content);
            instance.LastEventSequenceNumber = @event.SequenceNumber;
            instance.LastOccurred = @event.Occurred;
        }

        return instances
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .Select(entry => new AdHocProjectionInstance(
                entry.Key,
                entry.Value.Properties,
                entry.Value.LastEventSequenceNumber,
                entry.Value.LastOccurred))
            .ToList();
    }

    static void Merge(Dictionary<string, JsonElement> properties, JsonElement? content)
    {
        if (content is not { ValueKind: JsonValueKind.Object } value)
        {
            return;
        }

        foreach (var property in value.EnumerateObject())
        {
            properties[property.Name] = property.Value;
        }
    }

    sealed class InstanceState
    {
        public Dictionary<string, JsonElement> Properties { get; } = new(StringComparer.Ordinal);

        public ulong LastEventSequenceNumber { get; set; }

        public DateTimeOffset? LastOccurred { get; set; }
    }
}
