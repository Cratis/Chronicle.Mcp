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
    public Task<IEnumerable<EventType>> GetEventTypes(string eventStore) =>
        QueryAsync<EventType>($"api/event-store/{eventStore}/types");

    /// <summary>
    /// Gets all observers for a specific event store and namespace.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace to filter observers by.</param>
    /// <returns>A collection of <see cref="ObserverInformation"/>.</returns>
    public Task<IEnumerable<ObserverInformation>> GetObservers(string eventStore, string @namespace = "Default") =>
        QueryAsync<ObserverInformation>($"api/event-store/{eventStore}/{@namespace}/observers/all-observers");

    /// <summary>
    /// Gets all registered event stores.
    /// </summary>
    /// <returns>A collection of event store names.</returns>
    public Task<IEnumerable<string>> GetEventStores() =>
        QueryAsync<string>("api/event-stores");

    /// <summary>
    /// Gets all event sequences for a specific event store.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <returns>A collection of event sequence names.</returns>
    public Task<IEnumerable<string>> GetEventSequences(string eventStore) =>
        QueryAsync<string>($"api/event-store/{eventStore}/sequences");

    /// <summary>
    /// Gets appended events from a specific event sequence.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace to query.</param>
    /// <param name="eventSequenceId">The event sequence identifier.</param>
    /// <param name="eventSourceId">Optional event source identifier to filter by.</param>
    /// <returns>A collection of <see cref="AppendedEvent"/>.</returns>
    public Task<IEnumerable<AppendedEvent>> GetEvents(
        string eventStore,
        string @namespace = "Default",
        string eventSequenceId = "event-log",
        string? eventSourceId = null)
    {
        var url = $"api/event-store/{eventStore}/{@namespace}/sequence/{eventSequenceId}";
        if (!string.IsNullOrEmpty(eventSourceId))
        {
            url += $"?eventSourceId={Uri.EscapeDataString(eventSourceId)}";
        }

        return QueryAsync<AppendedEvent>(url);
    }

    /// <summary>
    /// Gets all recommendations for a specific event store and namespace.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace to query.</param>
    /// <returns>A collection of <see cref="Recommendation"/>.</returns>
    public Task<IEnumerable<Recommendation>> GetRecommendations(string eventStore, string @namespace = "Default") =>
        QueryAsync<Recommendation>($"api/event-store/{eventStore}/{@namespace}/recommendations");

    /// <summary>
    /// Gets all identities for a specific event store and namespace.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace to query.</param>
    /// <returns>A collection of <see cref="Identity"/>.</returns>
    public Task<IEnumerable<Identity>> GetIdentities(string eventStore, string @namespace = "Default") =>
        QueryAsync<Identity>($"api/event-store/{eventStore}/{@namespace}/identities");

    /// <summary>
    /// Gets all event type registrations including schemas for a specific event store.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <returns>A collection of <see cref="EventTypeRegistration"/>.</returns>
    public Task<IEnumerable<EventTypeRegistration>> GetEventTypeRegistrations(string eventStore) =>
        QueryAsync<EventTypeRegistration>($"api/event-store/{eventStore}/types/registrations");

    /// <summary>
    /// Replays an observer from the beginning of its event sequence.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the observer.</param>
    /// <param name="observerId">The unique identifier of the observer to replay.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ReplayObserver(string eventStore, string @namespace, string observerId) =>
        CommandAsync($"api/event-store/{eventStore}/observers/{@namespace}/replay/{observerId}");

    /// <summary>
    /// Replays a specific partition of an observer.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the observer.</param>
    /// <param name="observerId">The unique identifier of the observer.</param>
    /// <param name="partition">The partition identifier to replay.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ReplayObserverPartition(string eventStore, string @namespace, string observerId, string partition) =>
        CommandAsync($"api/event-store/{eventStore}/observers/{@namespace}/replay/{observerId}/{partition}");

    /// <summary>
    /// Attempts to recover a failed partition for an observer.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the observer.</param>
    /// <param name="observerId">The unique identifier of the observer.</param>
    /// <param name="partition">The partition identifier to recover.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RecoverFailedPartition(string eventStore, string @namespace, string observerId, string partition) =>
        CommandAsync($"api/event-store/{eventStore}/observers/{@namespace}/failed-partitions/{observerId}/try-recover-failed-partition/{partition}");

    /// <summary>
    /// Performs (accepts) a recommendation.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the recommendation.</param>
    /// <param name="recommendationId">The unique identifier of the recommendation to perform.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task PerformRecommendation(string eventStore, string @namespace, string recommendationId) =>
        CommandAsync($"api/event-store/{eventStore}/{@namespace}/recommendations/{recommendationId}/perform");

    /// <summary>
    /// Ignores (dismisses) a recommendation.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the recommendation.</param>
    /// <param name="recommendationId">The unique identifier of the recommendation to ignore.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task IgnoreRecommendation(string eventStore, string @namespace, string recommendationId) =>
        CommandAsync($"api/event-store/{eventStore}/{@namespace}/recommendations/{recommendationId}/ignore");

    /// <summary>
    /// Resumes a stopped job.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the job.</param>
    /// <param name="jobId">The unique identifier of the job to resume.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ResumeJob(string eventStore, string @namespace, string jobId) =>
        CommandAsync($"api/event-store/{eventStore}/{@namespace}/jobs/{jobId}/resume");

    /// <summary>
    /// Stops a running job.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the job.</param>
    /// <param name="jobId">The unique identifier of the job to stop.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StopJob(string eventStore, string @namespace, string jobId) =>
        CommandAsync($"api/event-store/{eventStore}/{@namespace}/jobs/{jobId}/stop");

    /// <summary>
    /// Deletes a job.
    /// </summary>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the job.</param>
    /// <param name="jobId">The unique identifier of the job to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteJob(string eventStore, string @namespace, string jobId) =>
        CommandAsync($"api/event-store/{eventStore}/{@namespace}/jobs/{jobId}/delete");

    /// <summary>
    /// Executes a query against the Chronicle REST API, handling the QueryResult envelope
    /// and gracefully returning empty collections on server errors.
    /// </summary>
    /// <typeparam name="T">The type of data items in the result.</typeparam>
    /// <param name="url">The relative URL to query.</param>
    /// <returns>A collection of result items, or empty if the server returned an error.</returns>
    async Task<IEnumerable<T>> QueryAsync<T>(string url)
    {
        using var response = await httpClient.GetAsync(url);
        var result = await response.Content.ReadFromJsonAsync<QueryResult<T>>(_jsonOptions);

        return result?.Data ?? [];
    }

    /// <summary>
    /// Executes a command (POST with no body) against the Chronicle REST API.
    /// </summary>
    /// <param name="url">The relative URL to post to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="HttpRequestException">The exception that is thrown when the Chronicle server returns an error response.</exception>
    async Task CommandAsync(string url)
    {
        using var response = await httpClient.PostAsync(url, null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;
            throw new HttpRequestException(
                $"Chronicle command failed with status {statusCode} ({response.StatusCode}) for '{url}'. " +
                "Response: " + body + ". Verify the event store, namespace, and resource IDs are correct. " +
                "Use the corresponding GET tool to list available resources first.");
        }
    }
}
