// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Represents the complete set of documents for a published Screenplay model.
/// </summary>
public sealed class ScreenplayModelManifestOptions
{
    /// <summary>
    /// Gets or sets the published model name, matched exactly.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the configured root alias, matched exactly.
    /// </summary>
    public string? Root { get; set; }

    /// <summary>
    /// Gets or sets the root-level entry document name.
    /// </summary>
    public string? Entry { get; set; }

    /// <summary>
    /// Gets or sets the relative paths of all documents, including the entry document.
    /// </summary>
    public IList<string> Files { get; set; } = new List<string>();
}
