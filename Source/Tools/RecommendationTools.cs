// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Recommendations;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Tools for inspecting recommendations in Chronicle.
/// </summary>
[McpServerToolType]
public static class RecommendationTools
{
    /// <summary>
    /// Lists active recommendations for an event store and namespace.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <returns>A concise descriptor for each recommendation.</returns>
    [McpServerTool(Name = "list_recommendations")]
    [Description("Lists active recommendations from the Chronicle server. Recommendations are automated suggestions for maintenance tasks such as rerunning projections or handling schema migrations.")]
    public static async Task<IEnumerable<RecommendationDescriptor>> ListRecommendations(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null)
    {
        var recommendations = await services.Recommendations.GetRecommendations(new GetRecommendationsRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace)
        });

        return recommendations.Select(recommendation => new RecommendationDescriptor(
            recommendation.Id,
            recommendation.Name,
            recommendation.Type,
            recommendation.Description,
            (DateTimeOffset?)recommendation.Occurred));
    }
}
