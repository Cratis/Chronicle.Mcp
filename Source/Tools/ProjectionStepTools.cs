// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.ReadModelExplorer;
using Cratis.Chronicle.Contracts.ReadModels;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Tools for stepping a registered projection through the history of one read model instance.
/// </summary>
[McpServerToolType]
public static class ProjectionStepTools
{
    /// <summary>
    /// The number of events accepted when no limit is given.
    /// </summary>
    public const int DefaultMaxEvents = 100;

    /// <summary>
    /// The largest number of events accepted, whatever limit is given.
    /// </summary>
    public const int HardMaxEvents = 500;

    const string EventGrouping = "Event";

    /// <summary>
    /// Steps a registered projection through the history of one read model instance, one event at a time.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="readModel">The read model container name or identifier.</param>
    /// <param name="readModelKey">The key of the read model instance.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="eventSequenceId">The event sequence to read. Defaults to event-log.</param>
    /// <param name="maxEvents">The largest number of events to accept.</param>
    /// <param name="cancellationToken">The cancellation token for the query.</param>
    /// <returns>The steps, or an explicit status when the history cannot be stepped.</returns>
    [McpServerTool(Name = "step_projection", ReadOnly = true, OpenWorld = false)]
    [Description("Steps a registered, server-owned projection through the history of one read model instance: the Chronicle server replays the real projection and returns the event and the resulting instance after each event, in order. The server reads the whole history for the key before returning, so the event limit bounds the output, not the server's work; use it for histories known to be small. Histories over the limit are refused, never truncated. Changed properties, unchanged and removed are derived by comparing consecutive snapshots, not evidence that a mapping fired. Content is store data released by the server under its compliance policy. Reducers and client-owned read models are not supported. Nothing is written.")]
    public static async Task<ProjectionStepResult> StepProjection(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The read model container name or identifier (see list_read_models).")] string readModel,
        [Description("The key of the read model instance, usually the event source id.")] string readModelKey,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("The event sequence to read. Defaults to event-log.")] string eventSequenceId = "event-log",
        [Description("The largest number of events to accept. Defaults to 100 and is capped at 500. Longer histories are refused, not truncated.")] int? maxEvents = null,
        CancellationToken cancellationToken = default)
    {
        var invalid = Validate(readModel, readModelKey, maxEvents);
        if (invalid is not null)
        {
            return Rejected(ProjectionStepStatus.InvalidRequest, readModel, readModelKey, invalid);
        }

        var store = configuration.ResolveEventStore(eventStore);
        var definitions = await services.ReadModels.GetDefinitions(new GetDefinitionsRequest { EventStore = store }, cancellationToken);
        var definition = definitions.ReadModels.FirstOrDefault(candidate =>
            string.Equals(candidate.ContainerName, readModel, StringComparison.Ordinal) ||
            string.Equals(candidate.Type?.Identifier, readModel, StringComparison.Ordinal));

        if (definition is null)
        {
            return Rejected(ProjectionStepStatus.UnknownReadModel, readModel, readModelKey, $"No read model '{readModel}' is registered in event store '{store}'.");
        }

        if (definition.ObserverType != ReadModelObserverType.Projection || definition.Owner == ReadModelOwner.Client)
        {
            return Rejected(ProjectionStepStatus.UnsupportedReadModel, readModel, readModelKey, $"Read model '{readModel}' is a {definition.ObserverType} owned by {definition.Owner}; only server-owned projections can be stepped.");
        }

        var snapshots = QueryResults.Unwrap(await services.ReadModelExplorer.AllSnapshotsForReadModel(
            new AllSnapshotsForReadModelRequest
            {
                EventStore = store,
                Namespace = configuration.ResolveNamespace(@namespace),
                ReadModel = definition.Type?.Identifier ?? readModel,
                ReadModelKey = readModelKey,
                EventSequenceId = eventSequenceId,
                Grouping = EventGrouping
            },
            cancellationToken));

        return ProjectionSteps.Build(readModel, readModelKey, [.. snapshots ?? []], Math.Min(maxEvents ?? DefaultMaxEvents, HardMaxEvents));
    }

    static string? Validate(string readModel, string readModelKey, int? maxEvents)
    {
        if (string.IsNullOrWhiteSpace(readModel))
        {
            return "A read model is required.";
        }

        if (string.IsNullOrWhiteSpace(readModelKey))
        {
            return "A read model key is required.";
        }

        return maxEvents <= 0 ? "maxEvents must be greater than zero when given." : null;
    }

    static ProjectionStepResult Rejected(string status, string readModel, string readModelKey, string note) =>
        new(status, readModel, readModelKey, 0, [], note);
}
