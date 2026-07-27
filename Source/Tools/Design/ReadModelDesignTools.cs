// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Contracts.Projections;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// The flagship design-time capability: turn a set of event types (which an agent resolves from a plain
/// natural-language request) into a reviewable read model + model-bound projection, grounded in the
/// events' real schema. The output is always a proposal to review, never a silent write.
/// </summary>
[McpServerToolType]
public static class ReadModelDesignTools
{
    /// <summary>
    /// Scaffolds a read model and its model-bound projection from one or more registered event types.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="readModelName">The name of the read model to generate.</param>
    /// <param name="eventTypes">A comma-separated list of event type ids to project from.</param>
    /// <param name="codeNamespace">The C# namespace to generate the read model into.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <returns>The scaffold, including grounded fields, existing-projection warnings, and generated code.</returns>
    [McpServerTool(Name = "scaffold_read_model")]
    [Description("Generates a reviewable read model + model-bound projection from one or more event types, grounded in the events' real schema (field names and types come from the store, never guesses). Resolve the event types from the user's request first — with list_event_types, describe_event_type, or generate_event_catalog — then pass them here. Surfaces existing projections that already read these events so a duplicate is not created. When none of the requested event types are registered it returns no code and says so rather than fabricating a shape.")]
    public static async Task<ReadModelScaffold> ScaffoldReadModel(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The name of the read model to generate (e.g. Users).")] string readModelName,
        [Description("A comma-separated list of event type ids to project from (e.g. UserRegistered,UserEmailVerified).")] string eventTypes,
        [Description("The C# namespace to generate the read model into. Defaults to ReadModels.")] string codeNamespace = "ReadModels",
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null)
    {
        var resolvedEventStore = configuration.ResolveEventStore(eventStore);
        var requested = SplitNames(eventTypes);
        var notes = new List<string>();

        if (requested.Count == 0)
        {
            notes.Add("No event types were provided. Pass one or more event type ids to ground the read model.");
            return new ReadModelScaffold(readModelName, codeNamespace, null, [], [], [], [], notes);
        }

        var registrations = (await services.EventTypes.GetAllRegistrations(new GetAllEventTypesRequest { EventStore = resolvedEventStore })).ToList();

        var resolved = new List<EventTypeRegistration>();
        var unresolved = new List<string>();
        foreach (var name in requested)
        {
            var registration = DesignIntrospection.ResolveRegistration(registrations, name);
            if (registration is null)
            {
                unresolved.Add(name);
            }
            else
            {
                resolved.Add(registration);
            }
        }

        if (resolved.Count == 0)
        {
            notes.Add("None of the requested event types are registered in this event store, so a read model cannot be grounded. Verify the names with list_event_types or generate_event_catalog.");
            return new ReadModelScaffold(readModelName, codeNamespace, null, [], [], unresolved, [], notes);
        }

        if (unresolved.Count > 0)
        {
            notes.Add($"Skipped unregistered event types: {string.Join(", ", unresolved)}.");
        }

        var resolvedIds = resolved.ConvertAll(registration => registration.Type.Id);
        var fields = ReadModelScaffolder.CollectFields(resolved, notes);

        if (fields.Count == 0)
        {
            notes.Add("The resolved event types carry no schema properties beyond identity, so the read model has only its key. Add fields once the events carry data.");
        }

        var existing = await FindExistingProjections(services, resolvedEventStore, readModelName, resolvedIds);
        if (existing.Count > 0)
        {
            notes.Add("One or more existing projections already read these events or target this read model. Review them before applying the scaffold to avoid a duplicate.");
        }

        var code = ReadModelScaffolder.Generate(readModelName, codeNamespace, resolvedIds, fields);

        return new ReadModelScaffold(readModelName, codeNamespace, code, fields, resolvedIds, unresolved, existing, notes);
    }

    static async Task<IReadOnlyList<ExistingProjectionMatch>> FindExistingProjections(IServices services, string eventStore, string readModelName, IReadOnlyList<string> resolvedIds)
    {
        var projections = await services.Projections.GetAllDefinitions(new GetAllDefinitionsRequest { EventStore = eventStore });
        var resolvedIdSet = resolvedIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return projections
            .Select(projection => (projection, overlap: DesignIntrospection.CollectConsumedEventTypeIds(projection).Where(resolvedIdSet.Contains).Order(StringComparer.OrdinalIgnoreCase).ToList()))
            .Where(candidate => candidate.overlap.Count > 0 || string.Equals(candidate.projection.ReadModel, readModelName, StringComparison.OrdinalIgnoreCase))
            .Select(candidate => new ExistingProjectionMatch(candidate.projection.Identifier, candidate.projection.ReadModel, candidate.overlap))
            .ToList();
    }

    static List<string> SplitNames(string input) =>
        string.IsNullOrWhiteSpace(input)
            ? []
            : input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
