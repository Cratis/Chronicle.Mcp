// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// An existing projection that overlaps with a scaffold request, surfaced to avoid duplicating work.
/// </summary>
/// <param name="ProjectionIdentifier">The identifier of the existing projection.</param>
/// <param name="ReadModel">The read model the existing projection targets.</param>
/// <param name="OverlappingEventTypes">The event types the existing projection shares with the request.</param>
public record ExistingProjectionMatch(string ProjectionIdentifier, string ReadModel, IReadOnlyList<string> OverlappingEventTypes);
