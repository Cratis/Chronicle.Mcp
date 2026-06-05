// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A concise description of a registered event type.
/// </summary>
/// <param name="Id">The unique identifier of the event type.</param>
/// <param name="Generation">The current generation of the event type.</param>
/// <param name="Tombstone">Whether the event type represents a tombstone.</param>
/// <param name="Owner">The owner of the event type (e.g. Client or Chronicle).</param>
/// <param name="Source">The source of the event type registration (e.g. Code).</param>
public record EventTypeDescriptor(string Id, uint Generation, bool Tombstone, string Owner, string Source);
