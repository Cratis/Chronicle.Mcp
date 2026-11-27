// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Represents the Screenplay models that an operator publishes for conformance audits.
/// </summary>
public sealed class ScreenplayModelCatalogOptions
{
    /// <summary>
    /// Gets or sets absolute model directory paths keyed by root alias.
    /// </summary>
    public IDictionary<string, string> Roots { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets published model manifests keyed by catalog name.
    /// </summary>
    public IDictionary<string, ScreenplayModelManifestOptions> Models { get; set; } = new Dictionary<string, ScreenplayModelManifestOptions>(StringComparer.Ordinal);
}
