// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// The result of an ad-hoc projection — instances materialized on demand from the selected
/// events, without registering anything in the store.
/// </summary>
/// <param name="EventStore">The event store the events were read from.</param>
/// <param name="Namespace">The namespace the events were read in.</param>
/// <param name="EventTypes">The event types that were folded.</param>
/// <param name="RemovedWithEventTypes">The event types that removed instances.</param>
/// <param name="Instances">The materialized instances, capped at the requested limit.</param>
/// <param name="TotalInstances">The total number of instances materialized before capping.</param>
/// <param name="FoldedEvents">The number of events that were folded.</param>
/// <param name="Guidance">Instructions for presenting the result and refining the projection.</param>
public record AdHocProjectionResult(
    string EventStore,
    string Namespace,
    IReadOnlyList<string> EventTypes,
    IReadOnlyList<string> RemovedWithEventTypes,
    IReadOnlyList<AdHocProjectionInstance> Instances,
    int TotalInstances,
    int FoldedEvents,
    string Guidance);
