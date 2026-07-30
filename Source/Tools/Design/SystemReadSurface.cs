// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A read surface of the system — a read model built by a projection or reducer. Read surfaces
/// reveal what the business actually watches: the questions the system exists to answer.
/// </summary>
/// <param name="Name">The read model name, or the observer identifier when no read model name is known.</param>
/// <param name="BuiltBy">How the surface is built: Projection or Reducer.</param>
/// <param name="EventTypes">The event types the surface is derived from.</param>
public record SystemReadSurface(
    string Name,
    string BuiltBy,
    IReadOnlyList<string> EventTypes);
