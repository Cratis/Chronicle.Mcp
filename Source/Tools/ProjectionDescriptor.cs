// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A concise description of a projection definition.
/// </summary>
/// <param name="Identifier">The identifier of the projection.</param>
/// <param name="ReadModel">The read model the projection projects into.</param>
/// <param name="EventSequenceId">The event sequence the projection observes.</param>
/// <param name="IsActive">Whether the projection is active.</param>
/// <param name="IsRewindable">Whether the projection can be rewound (replayed).</param>
/// <param name="AutoMap">The auto-mapping strategy used by the projection.</param>
public record ProjectionDescriptor(
    string Identifier,
    string ReadModel,
    string EventSequenceId,
    bool IsActive,
    bool IsRewindable,
    string AutoMap);
