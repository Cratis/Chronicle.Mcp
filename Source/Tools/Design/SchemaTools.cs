// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Design-time tools for inspecting the real shape (schema) of registered event types. This is the
/// grounding primitive the other design-time tools build on: field names and types come from the
/// store, never from a guess.
/// </summary>
[McpServerToolType]
public static class SchemaTools
{
    /// <summary>
    /// Describes the schema of a registered event type, listing its properties with their types.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventType">The event type id/name to describe.</param>
    /// <param name="generation">The optional specific generation; the latest is used when omitted.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <returns>The event type's schema, or null when no matching event type is registered.</returns>
    [McpServerTool(Name = "describe_event_type")]
    [Description("Describes a registered event type's real schema — every property with its JSON type and a suggested C# type — read from the store's event type registry. Use this to ground any generated projection, read model, or spec in the event's actual fields instead of guessing. Returns null when the event type is not registered.")]
    public static async Task<EventTypeSchemaDescriptor?> DescribeEventType(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event type id/name to describe (e.g. UserRegistered).")] string eventType,
        [Description("The optional specific generation to describe. The latest generation is used when omitted.")] uint? generation = null,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null)
    {
        var registrations = await services.EventTypes.GetAllRegistrations(new GetAllEventTypesRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore)
        });

        var registration = DesignIntrospection.ResolveRegistration(registrations, eventType, generation);
        if (registration is null)
        {
            return null;
        }

        return new EventTypeSchemaDescriptor(
            registration.Type.Id,
            registration.Type.Generation,
            registration.Type.Tombstone,
            registration.Owner.ToString(),
            registration.Source.ToString(),
            string.IsNullOrEmpty(registration.EventStore) ? null : registration.EventStore,
            EventSchemaParser.Parse(registration.Schema));
    }
}
