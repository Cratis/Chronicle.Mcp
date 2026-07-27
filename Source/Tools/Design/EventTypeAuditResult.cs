// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// The result of auditing an event store's event types against everything that consumes them.
/// </summary>
/// <param name="EventStore">The event store that was audited.</param>
/// <param name="Namespace">The namespace the observers were audited in.</param>
/// <param name="TotalEventTypes">The number of registered event types considered.</param>
/// <param name="Unconsumed">Event types that no projection, reducer, or reactor reads — data written that nothing consumes.</param>
/// <param name="DanglingReferences">Consumers that reference an event type id no longer present in the registry.</param>
public record EventTypeAuditResult(
    string EventStore,
    string Namespace,
    int TotalEventTypes,
    IReadOnlyList<UnconsumedEventType> Unconsumed,
    IReadOnlyList<DanglingEventTypeReference> DanglingReferences);
