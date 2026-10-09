// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Represents the deployment's principal and bounded mutation policy.
/// </summary>
public class MutationAuthorization
{
    /// <summary>
    /// Gets or sets the identity this process acts for.
    /// </summary>
    public string Principal { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the policy's expiration time.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the revoked identities.
    /// </summary>
    public IList<string> RevokedPrincipals { get; set; } = [];

    /// <summary>
    /// Gets or sets the grants narrowing Chronicle's authority.
    /// </summary>
    public IList<MutationGrant> Grants { get; set; } = [];
}
