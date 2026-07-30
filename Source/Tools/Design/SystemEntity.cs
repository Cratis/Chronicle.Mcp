// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// An entity deduced from the event type names in a store — the subject that a cluster of
/// events describe the life of.
/// </summary>
/// <param name="Name">The inferred entity name (e.g. "Author" from AuthorRegistered/AuthorRenamed).</param>
/// <param name="Events">The events describing the entity's life, ordered by lifecycle stage.</param>
public record SystemEntity(
    string Name,
    IReadOnlyList<SystemEntityEvent> Events);
