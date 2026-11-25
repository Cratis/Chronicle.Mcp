// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;
using Microsoft.Extensions.Logging;

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Emits value-free operator warnings. Logging is routed to stderr by the stdio host.
/// </summary>
/// <param name="credentials">The effective credential classifier.</param>
/// <param name="logger">The operator log.</param>
/// <param name="timeProvider">The policy clock.</param>
public partial class MutationPolicyWarnings(MutationCredentialPolicy credentials, ILogger<MutationPolicyWarnings> logger, TimeProvider timeProvider)
{
    /// <summary>
    /// Reports bounded local-policy migration and connection-identity limitations.
    /// </summary>
    /// <param name="settings">The current deployment settings.</param>
    public void Report(McpServerOptions settings)
    {
        if (settings.Profile != DeploymentProfile.Mutation) return;
        Identity(logger);
        var authorization = settings.Authorization;
        if (authorization.UsesLegacyPolicy) Legacy(logger);
        if (HasWildcards(authorization)) Wildcards(logger);
        if (authorization.ExpiresAt > timeProvider.GetUtcNow().AddDays(90)) LongExpiry(logger);
        if (credentials.UsesBearerCredential) BearerCredential(logger);
        if (authorization.AllowDevelopmentCredentials && credentials.UsesDevelopmentCredentials) DevelopmentCredentials(logger);
    }

    static bool HasWildcards(MutationAuthorization authorization) =>
        authorization.Grants.Any(grant => grant.EventStore == "*" || grant.Namespace == "*") ||
        authorization.Allowlist?.Any(entry => entry.EventStore == "*" || entry.Namespace == "*") == true;

    [LoggerMessage(LogLevel.Warning, "Mutation policy is only a local safety bound. Connection credentials identify the actual Chronicle caller; a client-credentials grant is not a delegated human identity. Chronicle remains authoritative.")]
    private static partial void Identity(ILogger logger);

    [LoggerMessage(LogLevel.Warning, "Legacy Principal, Grants, and RevokedPrincipals are deprecated local policy selectors, not authenticated identities. Migrate deliberately to Allowlist; removal also removes legacy target and revocation restrictions.")]
    private static partial void Legacy(ILogger logger);

    [LoggerMessage(LogLevel.Warning, "Mutation policy contains wildcard store or namespace scopes. Prefer least-privilege bounds.")]
    private static partial void Wildcards(ILogger logger);

    [LoggerMessage(LogLevel.Warning, "Mutation policy expires more than 90 days from now. Prefer a short-lived policy.")]
    private static partial void LongExpiry(ILogger logger);

    [LoggerMessage(LogLevel.Warning, "API keys and reused CLI login tokens for Mutation are deprecated. Configure dedicated least-privilege client credentials instead.")]
    private static partial void BearerCredential(ILogger logger);

    [LoggerMessage(LogLevel.Warning, "DANGER: Mutation is using well-known development client credentials. Restrict this deployment to isolated local development.")]
    private static partial void DevelopmentCredentials(ILogger logger);
}
