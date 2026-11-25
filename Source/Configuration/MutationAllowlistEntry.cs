// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Permits exact mutating operations across jobs in one store and namespace scope.
/// </summary>
public class MutationAllowlistEntry
{
    /// <summary>
    /// Gets or sets the event store, or * for all stores.
    /// </summary>
    public string EventStore { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the namespace, or * for all namespaces.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets exact mutating tool names. Missing, empty, and wildcard operations are invalid.
    /// </summary>
    public IList<string> Operations { get; set; } = [];
}
