// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Screenplay.Workspaces;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Validates and copies the complete manifest before acquiring any directory handles.
/// </summary>
/// <param name="roots">The validated root paths by alias.</param>
/// <param name="models">The validated model manifests by name.</param>
internal sealed class CatalogManifests(IReadOnlyDictionary<string, string> roots, IReadOnlyDictionary<string, CatalogManifests.Manifest> models)
{
    const int MaxRoots = 16;
    const int MaxModels = 64;
    const int MaxFiles = 16;

    /// <summary>Gets the validated root paths by alias.</summary>
    internal IReadOnlyDictionary<string, string> Roots { get; } = roots;

    /// <summary>Gets the validated model manifests by name.</summary>
    internal IReadOnlyDictionary<string, Manifest> Models { get; } = models;

    /// <summary>Copies valid settings, or refuses the entire catalog before any paths are opened.</summary>
    /// <param name="catalog">The configured catalog.</param>
    /// <returns>The validated snapshot or null.</returns>
    internal static CatalogManifests? Freeze(ScreenplayModelCatalogOptions? catalog)
    {
        if (catalog?.Roots is not { Count: > 0 and <= MaxRoots } configuredRoots) return null;
        if (catalog.Models is not { Count: > 0 and <= MaxModels } configuredModels) return null;
        if (!ValidRoots(configuredRoots) || !DistinctNames(configuredRoots.Keys)) return null;
        var roots = new Dictionary<string, string>(configuredRoots, StringComparer.Ordinal);
        var models = FreezeModels(roots, configuredModels);
        return models is null ? null : new(roots, models);
    }

    static bool ValidRoots(IDictionary<string, string> roots) =>
        roots.All(root => CatalogPaths.IsName(root.Key) && CatalogPaths.IsRoot(root.Value));

    static bool DistinctNames(IEnumerable<string> names) => names.ToHashSet(StringComparer.OrdinalIgnoreCase).Count == names.Count();

    static Dictionary<string, Manifest>? FreezeModels(Dictionary<string, string> roots, IList<ScreenplayModelManifestOptions> configuredModels)
    {
        var models = new Dictionary<string, Manifest>(StringComparer.Ordinal);
        foreach (var model in configuredModels)
        {
            if (!CatalogPaths.IsName(model?.Name)) return null;
            if (FreezeModel(roots, model) is not { } manifest || !models.TryAdd(model.Name, manifest)) return null;
        }

        return DistinctNames(models.Keys) ? models : null;
    }

    static Manifest? FreezeModel(Dictionary<string, string> roots, ScreenplayModelManifestOptions model)
    {
        if (model.Root is not { } root || !roots.ContainsKey(root)) return null;
        if (model.Files is not { Count: > 0 and <= MaxFiles } files) return null;
        if (!ValidEntry(model.Entry, files)) return null;
        var paths = FreezePaths(files);
        return paths is null ? null : new(root, model.Entry, paths);
    }

    static bool ValidEntry([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? entry, IList<string> files) =>
        entry?.Contains('/') == false && files.Contains(entry, StringComparer.Ordinal);

    static IReadOnlyList<PortablePlayPath>? FreezePaths(IList<string> files)
    {
        var paths = new HashSet<PortablePlayPath>(PortablePlayPath.CollisionComparer);
        foreach (var file in files)
        {
            if (!CatalogPaths.TryDocument(file, out var path) || !paths.Add(path)) return null;
        }

        return [.. paths.OrderBy(path => path.Value, StringComparer.Ordinal)];
    }

    /// <summary>Represents a checked, ordered manifest and its logical entry.</summary>
    /// <param name="Root">The configured root alias.</param>
    /// <param name="Entry">The root-level entry document path.</param>
    /// <param name="Files">The sorted logical document paths, including the entry.</param>
    internal sealed record Manifest(string Root, string Entry, IReadOnlyList<PortablePlayPath> Files);
}
