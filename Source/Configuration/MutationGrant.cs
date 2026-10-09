// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Grants specific operations in one store and namespace to a principal.
/// </summary>
public class MutationGrant
{
    /// <summary>
    /// Gets or sets the principal, matched exactly.
    /// </summary>
    public string Principal { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event store, or * for all stores.
    /// </summary>
    public string EventStore { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the namespace, or * for all namespaces.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets exact mutating tool names. Wildcards are not supported.
    /// </summary>
    public IList<string> Operations { get; set; } = [];

    /// <summary>
    /// Gets or sets exact target strings, or * for all targets.
    /// </summary>
    public IList<string> Targets { get; set; } = [];
}
