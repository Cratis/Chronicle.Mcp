// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Represents the deployment's bounded local mutation policy, not its connection identity.
/// </summary>
public class MutationAuthorization
{
    /// <summary>
    /// Gets or sets the deprecated local policy selector. This is not an authenticated identity.
    /// </summary>
    public string Principal { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the policy's expiration time.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the deprecated revoked local policy selectors.
    /// </summary>
    public IList<string> RevokedPrincipals { get; set; } = [];

    /// <summary>
    /// Gets or sets deprecated principal-based local grants, including their target restrictions.
    /// </summary>
    public IList<MutationGrant> Grants { get; set; } = [];

    /// <summary>
    /// Gets or sets principal-free store, namespace, and operation bounds. An empty list grants nothing.
    /// </summary>
    public IList<MutationAllowlistEntry>? Allowlist { get; set; }

    /// <summary>
    /// Gets or sets whether Mutation may use the well-known development client credentials.
    /// </summary>
    public bool AllowDevelopmentCredentials { get; set; }

    /// <summary>
    /// Gets or sets whether this fresh bind contains any legacy policy key, even an empty one.
    /// </summary>
    internal bool LegacyConfigured { get; set; }

    /// <summary>
    /// Gets whether legacy restrictions must remain in force, including programmatically supplied policy.
    /// </summary>
    internal bool UsesLegacyPolicy => LegacyConfigured || Allowlist is null ||
        !string.IsNullOrEmpty(Principal) || Grants.Count > 0 || RevokedPrincipals.Count > 0;
}
