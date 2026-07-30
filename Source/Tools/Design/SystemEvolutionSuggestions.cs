// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// The result of analyzing a system for event types worth introducing next — grounded gap
/// findings, not finished designs.
/// </summary>
/// <param name="EventStore">The event store that was analyzed.</param>
/// <param name="Entities">The entities the analysis was performed over.</param>
/// <param name="Suggestions">The suggested event types with the gap each one closes.</param>
/// <param name="Guidance">Instructions for refining the suggestions with domain knowledge.</param>
public record SystemEvolutionSuggestions(
    string EventStore,
    IReadOnlyList<SystemEntity> Entities,
    IReadOnlyList<EventTypeSuggestion> Suggestions,
    string Guidance);
