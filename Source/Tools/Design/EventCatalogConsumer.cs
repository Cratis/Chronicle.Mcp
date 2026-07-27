// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A consumer of an event type in the catalog.
/// </summary>
/// <param name="Type">The kind of consumer (e.g. Projection, Reducer, Reactor).</param>
/// <param name="Id">The identifier of the consumer.</param>
/// <param name="ReadModel">The read model the consumer builds, when it builds one; otherwise null.</param>
public record EventCatalogConsumer(string Type, string Id, string? ReadModel);
