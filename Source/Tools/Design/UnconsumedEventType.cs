// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// An event type that is registered but consumed by nothing.
/// </summary>
/// <param name="Id">The event type id.</param>
/// <param name="Generation">The current generation of the event type.</param>
/// <param name="Owner">The owner of the event type (Client types are the ones a developer usually cares about; Chronicle-owned types are framework-internal).</param>
/// <param name="Tombstone">Whether the event type is a tombstone.</param>
public record UnconsumedEventType(string Id, uint Generation, string Owner, bool Tombstone);
