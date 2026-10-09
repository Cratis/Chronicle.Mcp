// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Defines which capabilities a deployment exposes.
/// </summary>
public enum DeploymentProfile
{
    /// <summary>
    /// Exposes only tools explicitly declared read-only.
    /// </summary>
    ReadOnly = 0,

    /// <summary>
    /// Also exposes mutating tools, subject to authorization.
    /// </summary>
    Mutation = 1
}
