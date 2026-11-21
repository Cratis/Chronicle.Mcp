// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// The result of stepping a registered projection through the history of one read model instance.
/// </summary>
/// <param name="Status">The outcome; one of the <see cref="ProjectionStepStatus"/> values.</param>
/// <param name="ReadModel">The read model that was asked for.</param>
/// <param name="ReadModelKey">The read model key that was asked for.</param>
/// <param name="TotalEvents">The number of events the server returned for the key, also when the history is refused.</param>
/// <param name="Steps">One step per event, in the order the server returned them; empty unless the status is ok.</param>
/// <param name="Note">An explanation of the status, or of how the steps are to be read.</param>
public record ProjectionStepResult(
    string Status,
    string ReadModel,
    string ReadModelKey,
    int TotalEvents,
    IReadOnlyList<ProjectionStep> Steps,
    string? Note);
