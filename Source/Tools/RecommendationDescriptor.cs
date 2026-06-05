// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A concise description of a recommendation.
/// </summary>
/// <param name="Id">The recommendation identifier.</param>
/// <param name="Name">The recommendation name.</param>
/// <param name="Type">The recommendation type.</param>
/// <param name="Description">A human-readable description of the recommendation.</param>
/// <param name="Occurred">When the recommendation was raised.</param>
public record RecommendationDescriptor(
    Guid Id,
    string Name,
    string Type,
    string Description,
    DateTimeOffset? Occurred);
