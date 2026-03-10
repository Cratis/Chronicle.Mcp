// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Api;

/// <summary>
/// Represents an event type registered in Chronicle.
/// </summary>
/// <param name="Id">The unique identifier of the event type.</param>
/// <param name="Generation">The generation of the event type.</param>
/// <param name="Tombstone">Whether the event type is a tombstone.</param>
public record EventType(string Id, uint Generation, bool Tombstone);
