// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Screenplay.Workspaces;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Owns the startup manifest snapshot and its pinned root handles for the life of the host.
/// </summary>
internal sealed class ConformanceModelCatalog : IDisposable
{
    readonly CatalogManifests? _manifests;
    readonly Dictionary<string, ConfinedCatalogReader> _readers = new(StringComparer.Ordinal);

    /// <summary>Initializes a snapshot on the supported Linux ABIs and pins its configured roots.</summary>
    /// <param name="options">The startup deployment configuration.</param>
    public ConformanceModelCatalog(IOptions<McpServerOptions> options)
    {
        if (LinuxCatalogInterop.GetFlags() is not { } flags || CatalogManifests.Freeze(options.Value.ModelCatalog) is not { } manifests) return;
        var available = false;
        try
        {
            using var anchor = LinuxCatalogInterop.OpenAnchor(flags);
            if (anchor is null) return;
            foreach (var (alias, root) in manifests.Roots)
            {
                var directory = LinuxCatalogInterop.OpenRoot(flags, anchor, root[1..]);
                if (directory is null) return;
                _readers.Add(alias, new(flags, directory));
            }

            _manifests = manifests;
            available = true;
        }
        catch (Exception exception) when (MissingInterop(exception))
        {
            // The catalog is unavailable on a host without the required native interface.
        }
        finally
        {
            if (!available) Dispose();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var reader in _readers.Values) reader.Dispose();
        _readers.Clear();
    }

    /// <summary>Loads a published manifest by exact name or returns a sanitized refusal.</summary>
    /// <param name="name">The exact published model name.</param>
    /// <param name="cancellationToken">The caller's cancellation token.</param>
    /// <returns>The logical entry and documents, or a stable refusal code.</returns>
    internal (string? Entry, IReadOnlyList<WorkspaceDocument>? Documents, string? Error) Load(string name, CancellationToken cancellationToken)
    {
        if (!CatalogPaths.IsName(name) || _manifests is null || !_manifests.Models.TryGetValue(name, out var model)) return (null, null, "conformance_catalog_unavailable");
        try
        {
            return LoadDocuments(model, _readers[model.Root], cancellationToken);
        }
        catch (Exception exception) when (MissingInterop(exception))
        {
            return (null, null, "conformance_catalog_unavailable");
        }
    }

    static (string? Entry, IReadOnlyList<WorkspaceDocument>? Documents, string? Error) LoadDocuments(
        CatalogManifests.Manifest model, ConfinedCatalogReader reader, CancellationToken cancellationToken)
    {
        var documents = new List<WorkspaceDocument>(model.Files.Count);
        var remainingBytes = ConformanceModelSources.MaxSourceBytes;
        var remainingLines = ConformanceModelSources.MaxLines;
        foreach (var path in model.Files)
        {
            var (bytes, overBudget) = reader.Read(path.Value, remainingBytes, cancellationToken);
            if (bytes is null) return (null, null, overBudget ? "conformance_model_limit" : "conformance_catalog_unavailable");
            remainingBytes -= bytes.Length;
            var document = ConformanceModelSources.Document(path, bytes);
            if (!ConformanceModelSources.WithinLineLimits(document.Text, ref remainingLines)) return (null, null, "conformance_model_limit");
            documents.Add(document);
        }

        return (model.Entry, documents, null);
    }

    static bool MissingInterop(Exception exception) => exception is DllNotFoundException or EntryPointNotFoundException;
}
