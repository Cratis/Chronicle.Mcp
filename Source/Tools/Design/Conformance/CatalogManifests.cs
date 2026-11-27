// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Screenplay.Workspaces;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Validates and copies the complete manifest before acquiring any directory handles.
/// </summary>
internal sealed class CatalogManifests
{
    const int MaxRoots = 16;
    const int MaxModels = 64;
    const int MaxFiles = 16;
    readonly Dictionary<string, Manifest> _models = new(StringComparer.Ordinal);
    readonly Dictionary<string, string> _roots = new(StringComparer.Ordinal);

    /// <summary>Gets the validated root paths by alias.</summary>
    internal IReadOnlyDictionary<string, string> Roots => _roots;

    /// <summary>Gets the validated model manifests by name.</summary>
    internal IReadOnlyDictionary<string, Manifest> Models => _models;

    /// <summary>Copies valid settings, or refuses the entire catalog before any paths are opened.</summary>
    /// <param name="catalog">The configured catalog.</param>
    /// <returns>The validated snapshot or null.</returns>
    internal static CatalogManifests? Freeze(ScreenplayModelCatalogOptions? catalog)
    {
        if (catalog?.Roots is not { Count: > 0 and <= MaxRoots } roots || catalog.Models is not { Count: > 0 and <= MaxModels } models) return null;
        if (!ValidNames(roots.Keys) || !ValidNames(models.Keys)) return null;
        var frozen = new CatalogManifests();
        foreach (var (alias, root) in roots)
        {
            if (!CatalogPaths.IsRoot(root)) return null;
            frozen._roots.Add(alias, root);
        }

        foreach (var (name, model) in models)
        {
            if (FreezeModel(frozen._roots, model) is not { } manifest) return null;
            frozen._models.Add(name, manifest);
        }

        return frozen;
    }

    static bool ValidNames(ICollection<string> names) =>
        names.All(CatalogPaths.IsName) && names.Distinct(StringComparer.OrdinalIgnoreCase).Count() == names.Count;

    static Manifest? FreezeModel(Dictionary<string, string> roots, ScreenplayModelManifestOptions? model)
    {
        if (model?.Root is not { } root || !roots.ContainsKey(root) || model.Files is not { Count: > 0 and <= MaxFiles } files) return null;
        if (model.Entry is not { } entry || entry.Contains('/') || !files.Contains(entry, StringComparer.Ordinal)) return null;
        var paths = new HashSet<PortablePlayPath>(PortablePlayPath.CollisionComparer);
        foreach (var file in files)
        {
            if (!CatalogPaths.TryDocument(file, out var path) || !paths.Add(path)) return null;
        }

        return new(root, entry, [.. paths.OrderBy(path => path.Value, StringComparer.Ordinal)]);
    }

    /// <summary>Represents a checked, ordered manifest and its logical entry.</summary>
    /// <param name="Root">The configured root alias.</param>
    /// <param name="Entry">The root-level entry document path.</param>
    /// <param name="Files">The sorted logical document paths, including the entry.</param>
    internal sealed record Manifest(string Root, string Entry, IReadOnlyList<PortablePlayPath> Files);
}
