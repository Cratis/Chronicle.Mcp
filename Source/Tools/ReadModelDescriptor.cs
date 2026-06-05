// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A concise description of a read model definition.
/// </summary>
/// <param name="Identifier">The read model identifier.</param>
/// <param name="Generation">The current generation of the read model.</param>
/// <param name="ContainerName">The container (collection) name the read model is stored in.</param>
/// <param name="DisplayName">The friendly display name of the read model.</param>
/// <param name="ObserverType">The type of observer producing the read model.</param>
/// <param name="ObserverIdentifier">The identifier of the observer producing the read model.</param>
/// <param name="Owner">The owner of the read model (e.g. Client or Chronicle).</param>
/// <param name="Queryable">Whether instances can be queried from the Chronicle server.</param>
/// <param name="Source">The source of the read model registration.</param>
public record ReadModelDescriptor(
    string Identifier,
    uint Generation,
    string ContainerName,
    string DisplayName,
    string ObserverType,
    string ObserverIdentifier,
    string Owner,
    bool Queryable,
    string Source);
