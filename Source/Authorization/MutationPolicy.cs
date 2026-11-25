// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Validates local policy structure independently of authentication and expiration decisions.
/// </summary>
/// <param name="catalog">The catalog of known operations.</param>
public class MutationPolicy(ToolCatalog catalog)
{
    const string Path = $"{McpOptionsBinding.SectionPath}:Authorization";

    /// <summary>
    /// Finds invalid settings without returning their values. Mixed policies must both be valid.
    /// </summary>
    /// <param name="policy">The current policy.</param>
    /// <returns>Invalid setting paths.</returns>
    public IEnumerable<string> InvalidSettings(MutationAuthorization policy) =>
        InvalidLegacySettings(policy).Concat(InvalidAllowlistSettings(policy));

    IEnumerable<string> InvalidLegacySettings(MutationAuthorization policy)
    {
        if (!policy.UsesLegacyPolicy) yield break;
        if (!policy.Grants.Any(grant => string.Equals(grant.Principal, policy.Principal, StringComparison.Ordinal))) yield return $"{Path}:Grants";

        for (var index = 0; index < policy.Grants.Count; index++)
        {
            var grant = policy.Grants[index];
            var path = $"{Path}:Grants:{index}";
            if (string.IsNullOrWhiteSpace(grant.Principal)) yield return $"{path}:Principal";
            foreach (var failure in InvalidScope(path, grant.EventStore, grant.Namespace, grant.Operations)) yield return failure;
            if (grant.Targets.Count == 0) yield return $"{path}:Targets";
        }
    }

    IEnumerable<string> InvalidAllowlistSettings(MutationAuthorization policy)
    {
        if (policy.Allowlist is not { } entries) yield break;
        if (entries.Count == 0) yield return $"{Path}:Allowlist";

        for (var index = 0; index < entries.Count; index++)
        {
            var entry = entries[index];
            foreach (var failure in InvalidScope($"{Path}:Allowlist:{index}", entry.EventStore, entry.Namespace, entry.Operations)) yield return failure;
        }
    }

    IEnumerable<string> InvalidScope(string path, string eventStore, string @namespace, IList<string> operations)
    {
        if (string.IsNullOrWhiteSpace(eventStore)) yield return $"{path}:EventStore";
        if (string.IsNullOrWhiteSpace(@namespace)) yield return $"{path}:Namespace";
        if (operations.Count == 0 || operations.Any(operation => catalog.Find(operation) is not { IsReadOnly: false })) yield return $"{path}:Operations";
    }
}
