// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A page of read model instances.
/// </summary>
/// <param name="TotalCount">The total number of instances across all pages.</param>
/// <param name="Page">The 0-based page number returned.</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="Instances">The instances on this page, as JSON.</param>
public record ReadModelInstancesPage(
    long TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<JsonElement> Instances);
