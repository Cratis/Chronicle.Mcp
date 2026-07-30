// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// An event type placed within its entity's lifecycle, with the properties it carries.
/// </summary>
/// <param name="EventType">The event type id as registered in the store.</param>
/// <param name="Stage">Where in the entity's lifecycle the event sits, deduced from its name.</param>
/// <param name="Properties">The properties the event carries, parsed from its registered schema.</param>
public record SystemEntityEvent(
    string EventType,
    LifecycleStage Stage,
    IReadOnlyList<EventSchemaProperty> Properties);
