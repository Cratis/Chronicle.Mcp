// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A consumer that references an event type id which is not present in the store's event type registry.
/// </summary>
/// <param name="EventTypeId">The referenced-but-missing event type id.</param>
/// <param name="ConsumerType">The kind of consumer holding the reference (e.g. Projection, Reducer, Reactor).</param>
/// <param name="ConsumerId">The identifier of the consumer.</param>
public record DanglingEventTypeReference(string EventTypeId, string ConsumerType, string ConsumerId);
