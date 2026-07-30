// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Headline numbers for a described system.
/// </summary>
/// <param name="EventTypes">The number of distinct event types registered.</param>
/// <param name="Entities">The number of entities deduced from the event type names.</param>
/// <param name="ReadSurfaces">The number of read surfaces (projections and reducers).</param>
/// <param name="Automations">The number of automations (reactors and external observers).</param>
/// <param name="Namespaces">The number of namespaces in the event store.</param>
public record SystemStatistics(
    int EventTypes,
    int Entities,
    int ReadSurfaces,
    int Automations,
    int Namespaces);
