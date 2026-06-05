// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Text.Json;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.ReadModels;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Tools for working with read models in Chronicle.
/// </summary>
[McpServerToolType]
public static class ReadModelTools
{
    const string ClientOwner = "Client";

    /// <summary>
    /// Lists read model definitions registered in an event store.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <returns>A concise descriptor for each read model definition.</returns>
    [McpServerTool(Name = "list_read_models")]
    [Description("Lists all read model definitions registered in an event store. The 'queryable' field is false for client-owned read models whose state is stored by the client application rather than the Chronicle server; querying their instances will fail.")]
    public static async Task<IEnumerable<ReadModelDescriptor>> ListReadModels(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null)
    {
        var response = await services.ReadModels.GetDefinitions(new GetDefinitionsRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore)
        });

        return response.ReadModels.Select(readModel => new ReadModelDescriptor(
            readModel.Type?.Identifier ?? string.Empty,
            readModel.Type?.Generation ?? 0,
            readModel.ContainerName,
            readModel.DisplayName,
            readModel.ObserverType.ToString(),
            readModel.ObserverIdentifier,
            readModel.Owner.ToString(),
            !string.Equals(readModel.Owner.ToString(), ClientOwner, StringComparison.Ordinal),
            readModel.Source.ToString()));
    }

    /// <summary>
    /// Lists current instances of a read model with pagination.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="readModel">The read model container name to list instances for.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="page">The 0-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A page of read model instances.</returns>
    [McpServerTool(Name = "get_read_model_instances")]
    [Description("Lists the current instances of a read model as JSON, with pagination. Use to inspect the projected state of entities. Fails for client-owned read models (see 'queryable' in list_read_models).")]
    public static async Task<ReadModelInstancesPage> GetReadModelInstances(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The read model container name to list instances for.")] string readModel,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("The 0-based page number. Defaults to 0.")] int page = 0,
        [Description("The number of items per page. Defaults to 20.")] int pageSize = 20)
    {
        var response = await services.ReadModels.GetInstances(new GetInstancesRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace),
            ReadModel = readModel,
            Page = page,
            PageSize = pageSize
        });

        var instances = (response.Instances ?? [])
            .Select(TryParse)
            .Where(instance => instance.HasValue)
            .Select(instance => instance!.Value)
            .ToList();

        return new ReadModelInstancesPage(response.TotalCount, response.Page, response.PageSize, instances);
    }

    static JsonElement? TryParse(string instance)
    {
        if (string.IsNullOrWhiteSpace(instance))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(instance);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
