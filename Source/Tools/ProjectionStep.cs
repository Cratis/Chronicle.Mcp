// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// The state of a read model instance after the registered projection handled one more event.
/// </summary>
/// <param name="Step">The 1-based position of the step in the history.</param>
/// <param name="Occurred">When the event of the step occurred.</param>
/// <param name="CorrelationId">The correlation id of the event of the step.</param>
/// <param name="Events">The event handled in this step, as released by the server.</param>
/// <param name="Instance">The read model instance after the step, as JSON.</param>
/// <param name="ChangedProperties">Top-level properties that differ from the previous step. Derived by comparison, not evidence that a mapping fired.</param>
/// <param name="Unchanged">Whether the instance is the same as after the previous step. Derived by comparison.</param>
/// <param name="Removed">Whether the instance had state after the previous step and has none after this one. Derived by comparison.</param>
/// <param name="Note">An explanation when the step could not be compared, for instance when the instance is not valid JSON.</param>
public record ProjectionStep(
    int Step,
    DateTimeOffset? Occurred,
    Guid CorrelationId,
    IReadOnlyList<EventDescriptor> Events,
    JsonElement? Instance,
    IReadOnlyList<string> ChangedProperties,
    bool Unchanged,
    bool Removed,
    string? Note);
