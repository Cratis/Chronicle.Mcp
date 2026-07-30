// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A suggested event type to introduce next, grounded in a gap found in the store's current model.
/// </summary>
/// <param name="SuggestedName">The suggested event type name — past tense, one purpose, self-describing.</param>
/// <param name="Entity">The entity whose lifecycle the suggestion completes.</param>
/// <param name="Stage">The lifecycle stage the suggested event fills.</param>
/// <param name="Rationale">Why this event is suggested — the concrete gap it closes.</param>
/// <param name="SuggestedProperties">Properties the event could carry, drawn from the entity's existing schema vocabulary.</param>
public record EventTypeSuggestion(
    string SuggestedName,
    string Entity,
    LifecycleStage Stage,
    string Rationale,
    IReadOnlyList<EventSchemaProperty> SuggestedProperties);
