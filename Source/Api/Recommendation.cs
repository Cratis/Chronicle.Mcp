// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Api;

/// <summary>
/// Represents a recommendation from Chronicle.
/// </summary>
/// <param name="Id">The unique identifier of the recommendation.</param>
/// <param name="Name">The name of the recommendation.</param>
/// <param name="Description">The description of the recommendation.</param>
/// <param name="Type">The type of the recommendation.</param>
/// <param name="Occurred">When the recommendation occurred.</param>
public record Recommendation(
    Guid Id,
    string Name,
    string Description,
    string Type,
    DateTimeOffset Occurred);
