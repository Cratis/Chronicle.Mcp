// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Validates grant structure independently of authentication and expiration decisions.
/// </summary>
/// <param name="catalog">The catalog of known operations.</param>
public class MutationPolicy(ToolCatalog catalog)
{
    const string Path = $"{McpOptionsBinding.SectionPath}:Authorization";

    /// <summary>
    /// Finds invalid settings without returning their values.
    /// </summary>
    /// <param name="policy">The current policy.</param>
    /// <returns>Invalid setting paths.</returns>
    public IEnumerable<string> InvalidSettings(MutationAuthorization policy)
    {
        if (!policy.Grants.Any(grant => string.Equals(grant.Principal, policy.Principal, StringComparison.Ordinal)))
        {
            yield return $"{Path}:Grants";
        }

        for (var index = 0; index < policy.Grants.Count; index++)
        {
            var grant = policy.Grants[index];
            var path = $"{Path}:Grants:{index}";
            if (string.IsNullOrWhiteSpace(grant.Principal)) yield return $"{path}:Principal";
            if (string.IsNullOrWhiteSpace(grant.EventStore)) yield return $"{path}:EventStore";
            if (string.IsNullOrWhiteSpace(grant.Namespace)) yield return $"{path}:Namespace";
            if (grant.Operations.Count == 0 || grant.Operations.Any(operation => catalog.Find(operation) is not { IsReadOnly: false })) yield return $"{path}:Operations";
            if (grant.Targets.Count == 0) yield return $"{path}:Targets";
        }
    }
}
