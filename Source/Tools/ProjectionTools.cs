// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Projections;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Tools for working with projection definitions in Chronicle.
/// </summary>
[McpServerToolType]
public static class ProjectionTools
{
    /// <summary>
    /// Lists projection definitions registered in an event store.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <returns>A concise descriptor for each projection definition.</returns>
    [McpServerTool(Name = "list_projections")]
    [Description("Lists all projection definitions registered in an event store, including the read model they project into and their activity state. Use to audit registered projections.")]
    public static async Task<IEnumerable<ProjectionDescriptor>> ListProjections(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null)
    {
        var definitions = await services.Projections.GetAllDefinitions(new GetAllDefinitionsRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore)
        });

        return definitions.Select(definition => new ProjectionDescriptor(
            definition.Identifier,
            definition.ReadModel,
            definition.EventSequenceId,
            definition.IsActive,
            definition.IsRewindable,
            definition.AutoMap.ToString()));
    }
}
