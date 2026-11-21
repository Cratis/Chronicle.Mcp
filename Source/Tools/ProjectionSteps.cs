// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using Cratis.Chronicle.Contracts.ReadModelExplorer;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Turns the read model snapshots the server returned into projection steps, within an output budget.
/// </summary>
static class ProjectionSteps
{
    /// <summary>
    /// The largest combined size, in UTF-8 bytes, of instances and event content a result may carry.
    /// </summary>
    internal const int MaxPayloadBytes = 256 * 1024;

    const string DerivedNote = "Changed properties, unchanged and removed are derived by comparing consecutive snapshots; they are not evidence that a specific projection mapping fired. Content is store data released by the server under its compliance policy.";
    const string UnreadableInstanceNote = "The server returned an instance that is not valid JSON; no differences are derived for this step.";

    /// <summary>
    /// Builds the result for the snapshots of one read model key.
    /// </summary>
    /// <param name="readModel">The read model asked for.</param>
    /// <param name="readModelKey">The key asked for.</param>
    /// <param name="snapshots">The snapshots, in the order the server returned them.</param>
    /// <param name="maxEvents">The largest number of events to accept.</param>
    /// <returns>The steps, or an explicit refusal when the history is empty or over budget.</returns>
    internal static ProjectionStepResult Build(string readModel, string readModelKey, IReadOnlyList<ReadModelSnapshotResponse> snapshots, int maxEvents)
    {
        var totalEvents = snapshots.Sum(snapshot => snapshot.Events.Count());
        var refusal = Refusal(snapshots, totalEvents, maxEvents);
        if (refusal is not null)
        {
            var status = snapshots.Count == 0 ? ProjectionStepStatus.NoHistory : ProjectionStepStatus.RefusedOverBudget;
            return new(status, readModel, readModelKey, totalEvents, [], refusal);
        }

        var steps = new List<ProjectionStep>(snapshots.Count);
        var previous = ProjectionStepInstance.Empty;
        foreach (var snapshot in snapshots)
        {
            var current = ProjectionStepInstance.From(snapshot.Instance);
            steps.Add(ToStep(steps.Count + 1, snapshot, previous, current));
            previous = current;
        }

        return new(ProjectionStepStatus.Ok, readModel, readModelKey, totalEvents, steps, DerivedNote);
    }

    static string? Refusal(IReadOnlyList<ReadModelSnapshotResponse> snapshots, int totalEvents, int maxEvents)
    {
        if (snapshots.Count == 0)
        {
            return "The projection produced no snapshots for this key: it has no events for it, or the key does not exist.";
        }

        if (totalEvents > maxEvents)
        {
            return $"The history has {totalEvents} events, more than the limit of {maxEvents}. It was refused, not truncated. The server had already read the whole history to answer.";
        }

        var payloadBytes = snapshots.Sum(PayloadBytes);
        return payloadBytes > MaxPayloadBytes
            ? $"The history carries {payloadBytes} bytes of instances and event content, more than the limit of {MaxPayloadBytes}. It was refused, not truncated. The server had already read the whole history to answer."
            : null;
    }

    static long PayloadBytes(ReadModelSnapshotResponse snapshot) =>
        Bytes(snapshot.Instance) + snapshot.Events.Sum(@event => Bytes(@event.Content));

    static long Bytes(string? text) => Encoding.UTF8.GetByteCount(text ?? string.Empty);

    static ProjectionStep ToStep(int step, ReadModelSnapshotResponse snapshot, ProjectionStepInstance previous, ProjectionStepInstance current)
    {
        var events = snapshot.Events.Select(ToEvent).ToList();
        if (!previous.Readable || !current.Readable)
        {
            return new(step, (DateTimeOffset?)snapshot.Occurred, snapshot.CorrelationId, events, current.Value, [], false, false, current.Readable ? null : UnreadableInstanceNote);
        }

        var changed = ChangedProperties(previous.Value, current.Value);
        var unchanged = changed.Count == 0 && previous.HasState == current.HasState;
        var removed = previous.HasState && !current.HasState;
        return new(step, (DateTimeOffset?)snapshot.Occurred, snapshot.CorrelationId, events, current.Value, changed, unchanged, removed, null);
    }

    static EventDescriptor ToEvent(Event @event)
    {
        var context = @event.Context;
        return new(
            context.SequenceNumber,
            context.EventType?.Id ?? string.Empty,
            context.EventType?.Generation ?? 0,
            context.EventSourceId,
            (DateTimeOffset?)context.Occurred,
            context.CorrelationId,
            ProjectionStepInstance.From(@event.Content).Value);
    }

    static List<string> ChangedProperties(JsonElement? previous, JsonElement? current)
    {
        var before = Properties(previous);
        var after = Properties(current);
        return [.. before.Keys
            .Union(after.Keys, StringComparer.Ordinal)
            .Where(name => !before.TryGetValue(name, out var was) || !after.TryGetValue(name, out var @is) || !JsonElement.DeepEquals(was, @is))
            .Order(StringComparer.Ordinal)];
    }

    static Dictionary<string, JsonElement> Properties(JsonElement? element)
    {
        var properties = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        if (element is { ValueKind: JsonValueKind.Object } value)
        {
            foreach (var property in value.EnumerateObject())
            {
                properties[property.Name] = property.Value;
            }
        }

        return properties;
    }
}
