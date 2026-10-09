// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Applies explicit empty authorization values without retaining children from another provider.
/// </summary>
public static class EmptyAuthorizationConfiguration
{
    /// <summary>
    /// Treats empty objects as absent and empty collections as having no entries.
    /// </summary>
    /// <param name="section">The MCP configuration section.</param>
    /// <param name="options">The strictly bound options to normalize.</param>
    /// <exception cref="InvalidMcpConfiguration">Grant binding did not preserve the validated element count.</exception>
    public static void Apply(IConfiguration section, McpServerOptions options)
    {
        var authorization = section.GetSection(nameof(McpServerOptions.Authorization));
        if (IsEmpty(authorization))
        {
            options.Authorization = new();
            return;
        }

        var policy = options.Authorization;
        if (IsEmpty(authorization.GetSection(nameof(MutationAuthorization.RevokedPrincipals))))
        {
            policy.RevokedPrincipals = [];
        }

        var grants = authorization.GetSection(nameof(MutationAuthorization.Grants));
        if (IsEmpty(grants))
        {
            policy.Grants = [];
            return;
        }

        policy.Grants = NormalizeGrants(grants, policy.Grants);
    }

    static List<MutationGrant> NormalizeGrants(IConfigurationSection section, IList<MutationGrant> grants)
    {
        var configured = section.GetChildren().ToArray();
        if (configured.Length != grants.Count)
        {
            throw new InvalidMcpConfiguration($"{McpOptionsBinding.SectionPath}:Authorization:Grants");
        }

        var result = new List<MutationGrant>();
        for (var index = 0; index < configured.Length; index++)
        {
            var configuration = configured[index];
            if (!IsEmpty(configuration))
            {
                var grant = grants[index];
                if (IsEmpty(configuration.GetSection(nameof(MutationGrant.Operations)))) grant.Operations = [];
                if (IsEmpty(configuration.GetSection(nameof(MutationGrant.Targets)))) grant.Targets = [];
                result.Add(grant);
            }
        }

        return result;
    }

    static bool IsEmpty(IConfigurationSection section) => section.Value?.Length == 0 || !section.GetChildren().Any();
}
