// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Json;
using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Api;

/// <summary>
/// Client for interacting with the Chronicle REST API.
/// </summary>
/// <param name="httpClient">The <see cref="HttpClient"/> configured for Chronicle management API.</param>
public class ChronicleApiClient(HttpClient httpClient)
{
    static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Gets all event types for a specific event store.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <returns>A collection of <see cref="EventType"/>.</returns>
    public async Task<IEnumerable<EventType>> GetEventTypes(string eventStore)
    {
        var result = await httpClient.GetFromJsonAsync<QueryResult<EventType>>(
            $"api/event-store/{eventStore}/types", _jsonOptions);

        return result?.Data ?? [];
    }

    /// <summary>
    /// Gets all observers for a specific event store and namespace.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace to filter observers by.</param>
    /// <returns>A collection of <see cref="ObserverInformation"/>.</returns>
    public async Task<IEnumerable<ObserverInformation>> GetObservers(string eventStore, string @namespace = "Default")
    {
        var result = await httpClient.GetFromJsonAsync<QueryResult<ObserverInformation>>(
            $"api/event-store/{eventStore}/{@namespace}/observers/all-observers", _jsonOptions);

        return result?.Data ?? [];
    }
}
