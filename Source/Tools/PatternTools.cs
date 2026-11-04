// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Patterns;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Tools for querying learned behavior patterns in Chronicle.
/// </summary>
[McpServerToolType]
public static class PatternTools
{
    /// <summary>
    /// Gets matching behavior patterns in the server's authoritative order.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="groupingKey">The grouping key used when learning the patterns.</param>
    /// <param name="context">The current facet values to match.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="minimumConfidence">Minimum confidence; non-positive values let the server resolve its default.</param>
    /// <param name="maximumResults">Maximum result count; non-positive values let the server resolve its default.</param>
    /// <param name="cancellationToken">The cancellation token for the query.</param>
    /// <returns>The full matching pattern details without local ranking or filtering.</returns>
    [McpServerTool(Name = "get_patterns", ReadOnly = true, OpenWorld = false)]
    [Description("Gets learned behavior patterns matching a grouping key and context, in the server's authoritative order. Returns full pattern details; an action is an ordinary facet, not a separate field.")]
    public static async Task<IEnumerable<BehaviorPatternDetailsResponse>> GetPatterns(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The grouping key used when learning the patterns.")] string groupingKey,
        [Description("Current facet values to match. Keys and values are forwarded unchanged; an action, if present, is an ordinary facet.")] IDictionary<string, string> context,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("Minimum confidence, normally 0 to 1. Non-positive values let the backend resolve its configured default. Forwarded unchanged without local range validation.")] double minimumConfidence = 0,
        [Description("Maximum result count. Non-positive values let the backend resolve its configured default. Positive values are forwarded unchanged without a local cap.")] int maximumResults = 0,
        CancellationToken cancellationToken = default)
    {
        var result = await services.Patterns.MatchingPatterns(
            new MatchingPatternsRequest
            {
                EventStore = configuration.ResolveEventStore(eventStore),
                Namespace = configuration.ResolveNamespace(@namespace),
                GroupingKey = groupingKey,
                Context = context,
                MinimumConfidence = minimumConfidence,
                MaximumResults = maximumResults
            },
            cancellationToken);

        return QueryResults.Unwrap(result);
    }
}
