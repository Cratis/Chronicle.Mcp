// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Represents an allowed call or a denial with a stable code.
/// </summary>
/// <param name="Code">The denial code, or null for an allowed call.</param>
public record AuthorizationDecision(string? Code)
{
    /// <summary>
    /// Gets a value indicating whether the call is allowed.
    /// </summary>
    public bool IsAllowed => Code is null;
}
