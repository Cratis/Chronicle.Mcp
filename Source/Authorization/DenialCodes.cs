// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Stable machine-readable denial codes.
/// </summary>
public static class DenialCodes
{
    /// <summary>
    /// The target is missing or malformed.
    /// </summary>
    public const string BadRequest = "bad_request";

    /// <summary>
    /// No deployment principal is configured.
    /// </summary>
    public const string NotAuthenticated = "not_authenticated";

    /// <summary>
    /// The current grant policy is invalid.
    /// </summary>
    public const string PolicyInvalid = "policy_invalid";

    /// <summary>
    /// The policy has expired or has no expiration.
    /// </summary>
    public const string PolicyExpired = "policy_expired";

    /// <summary>
    /// The deployment principal was revoked.
    /// </summary>
    public const string PrincipalRevoked = "principal_revoked";

    /// <summary>
    /// No grant covers all request dimensions.
    /// </summary>
    public const string NotAuthorized = "not_authorized";

    /// <summary>
    /// Chronicle denied authentication or permission.
    /// </summary>
    public const string ChronicleDenied = "chronicle_denied";
}
