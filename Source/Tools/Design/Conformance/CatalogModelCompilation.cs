// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Screenplay.Diagnostics;
using Cratis.Screenplay.Files;
using Cratis.Screenplay.Semantics;
using Cratis.Screenplay.Syntax;
using Cratis.Screenplay.Workspaces;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Assembles the selected manifest from checked snapshots, without disk-based compilation or extension resolution.
/// </summary>
internal static class CatalogModelCompilation
{
    /// <summary>Preflights the closed document graph before binding any semantic model.</summary>
    /// <param name="entry">The root-level logical entry document path.</param>
    /// <param name="documents">The bounded and decoded snapshots.</param>
    /// <param name="applicationName">The optional caller-supplied name.</param>
    /// <param name="fallback">The deployment's application fallback name.</param>
    /// <returns>The compilation or refusal diagnostics.</returns>
    internal static (SemanticCompilation? Model, IEnumerable<Diagnostic> Diagnostics) Compile(
        string entry,
        IReadOnlyList<WorkspaceDocument> documents,
        string? applicationName,
        string fallback)
    {
        var contents = documents.Select(document => new PlayFileContent(document.Path.Value, document.Text));
        var assembled = new PlayFileCompiler().CompileApplication(entry, contents);
        var syntax = assembled.Result;
        if (!syntax.Success || syntax.Diagnostics.Any(diagnostic => diagnostic.Code == DiagnosticCodes.FileImportMatchesNothing)) return (null, syntax.Diagnostics);
        if (!ReachedAll(assembled.Sources, documents) || !IsSelfContained(syntax.Value!)) return (null, []);

        var name = applicationName ?? syntax.Value!.Domain?.Name ?? fallback;
        return ConformanceScope.Identifier(name) ? ConformanceModelSources.Bind(name, documents) : (null, []);
    }

    static bool ReachedAll(IEnumerable<PlayFileSource> reached, IEnumerable<WorkspaceDocument> documents) =>
        reached.Select(source => source.File.RelativePath).ToHashSet(StringComparer.Ordinal).SetEquals(documents.Select(document => document.Path.Value));

    static bool IsSelfContained(ApplicationSyntax application)
    {
        // Folder merge retains the authored patterns even when the resolver matched a normalized path.
        return !ConformanceModelSources.HasExternalReferences(application) && ValidImports(application.FileImports) &&
            application.Modules.All(module => ValidImports(module.FileImports) && module.Features.All(ValidFeature));
    }

    static bool ValidFeature(FeatureSyntax feature) => ValidImports(feature.FileImports) && feature.Features.All(ValidFeature);

    static bool ValidImports(IEnumerable<FileImportSyntax> imports) => imports.All(import => CatalogPaths.IsImport(import.Pattern));
}
