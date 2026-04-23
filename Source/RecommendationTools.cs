// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Mcp.Api;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp;

/// <summary>
/// Represents a set of tools for working with recommendations in Chronicle.
/// </summary>
[McpServerToolType]
public static class RecommendationTools
{
    /// <summary>
    /// Gets all recommendations for a specific event store and namespace.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store to get recommendations from.</param>
    /// <param name="namespace">The namespace to query. Defaults to 'Default'.</param>
    /// <returns>A collection of <see cref="Recommendation"/> with name, description, type and occurrence time.</returns>
    [McpServerTool(ReadOnly = true), Description("Gets all recommendations for a specific event store and namespace. Recommendations are actionable suggestions from Chronicle about potential issues or improvements.")]
    public static async Task<IEnumerable<Recommendation>> GetRecommendations(
        ChronicleApiClient chronicleApiClient,
        string eventStore,
        string? @namespace = "Default") =>
        await chronicleApiClient.GetRecommendations(eventStore, @namespace ?? "Default");

    /// <summary>
    /// Performs (accepts) a recommendation, applying its suggested action.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the recommendation.</param>
    /// <param name="recommendationId">The unique identifier of the recommendation to perform.</param>
    /// <returns>A confirmation message.</returns>
    [McpServerTool, Description("Performs (accepts) a recommendation, applying its suggested action. Use GetRecommendations first to see available recommendations and their IDs.")]
    public static async Task<string> PerformRecommendation(
        ChronicleApiClient chronicleApiClient,
        string eventStore,
        string @namespace,
        string recommendationId)
    {
        await chronicleApiClient.PerformRecommendation(eventStore, @namespace, recommendationId);

        return $"Recommendation '{recommendationId}' performed in event store '{eventStore}', namespace '{@namespace}'.";
    }

    /// <summary>
    /// Ignores (dismisses) a recommendation without applying its action.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the recommendation.</param>
    /// <param name="recommendationId">The unique identifier of the recommendation to ignore.</param>
    /// <returns>A confirmation message.</returns>
    [McpServerTool, Description("Ignores (dismisses) a recommendation without applying its action. The recommendation will be removed from the active list.")]
    public static async Task<string> IgnoreRecommendation(
        ChronicleApiClient chronicleApiClient,
        string eventStore,
        string @namespace,
        string recommendationId)
    {
        await chronicleApiClient.IgnoreRecommendation(eventStore, @namespace, recommendationId);

        return $"Recommendation '{recommendationId}' ignored in event store '{eventStore}', namespace '{@namespace}'.";
    }
}
