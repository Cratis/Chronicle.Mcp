// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Screenplay;
using Cratis.Screenplay.Comparison;
using Cratis.Screenplay.Diagnostics;
using Cratis.Screenplay.Semantics;
using Cratis.Screenplay.Syntax;
using Cratis.Screenplay.Workspaces;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Admits and compiles one self-contained inline model without file discovery.
/// </summary>
internal static class ConformanceModelSources
{
    /// <summary>The maximum total UTF-8 source bytes for one model.</summary>
    internal const int MaxSourceBytes = 64 * 1024;

    /// <summary>The maximum total source lines for one model.</summary>
    internal const int MaxLines = 4096;

    const string LogicalPath = "application.play";
    const int MaxLineBytes = 2048;
    const int MaxIndent = 128;
    static readonly UTF8Encoding _utf8 = new(false, true);

    /// <summary>
    /// Checks the source byte, line, and indentation limits before parsing.
    /// </summary>
    /// <param name="source">The inline document.</param>
    /// <returns>Whether the source meets the audit limits.</returns>
    internal static bool Admit(string? source)
    {
        if (string.IsNullOrWhiteSpace(source)) return false;
        if (source.Length > MaxSourceBytes || _utf8.GetByteCount(source) > MaxSourceBytes) return false;
        var remainingLines = MaxLines;
        return WithinLineLimits(source, ref remainingLines);
    }

    /// <summary>
    /// Compiles the inline document under the CLI's comparison identity convention.
    /// </summary>
    /// <param name="source">The admitted inline source.</param>
    /// <param name="applicationName">The optional explicit application name.</param>
    /// <param name="fallback">The configured name used when the model declares no domain.</param>
    /// <returns>The compiled model if valid, and any compiler diagnostics.</returns>
    internal static (SemanticCompilation? Model, IEnumerable<Diagnostic> Diagnostics) Compile(string source, string? applicationName, string fallback)
    {
        var syntax = new ScreenplayCompiler().Parse(source, LogicalPath);
        if (!syntax.Success) return (null, syntax.Diagnostics);
        var application = syntax.Value!;
        if (application.FileImports.Any() || HasExternalReferences(application)) return (null, []);
        var name = applicationName ?? application.Domain?.Name ?? fallback;
        if (!ConformanceScope.Identifier(name)) return (null, []);
        return Bind(name, [Document(PortablePlayPath.Parse(LogicalPath), _utf8.GetBytes(source))]);
    }

    /// <summary>Checks for domain imports or system extensions that could need ambient artifacts.</summary>
    /// <param name="application">The preflight syntax tree.</param>
    /// <returns>Whether an ambient reference is present.</returns>
    internal static bool HasExternalReferences(ApplicationSyntax application) =>
        application.Imports.Any() || application.Systems.Any(system => system.Extension is not null);

    /// <summary>Creates a strictly decoded document with the comparison CLI's stable key.</summary>
    /// <param name="path">The logical document path.</param>
    /// <param name="bytes">The admitted source bytes.</param>
    /// <returns>The immutable document snapshot.</returns>
    internal static WorkspaceDocument Document(PortablePlayPath path, ReadOnlySpan<byte> bytes)
    {
        // Keep the same stable document identity as the CLI's ComparisonInput.
        var key = $"file-{Convert.ToHexStringLower(SHA256.HashData(_utf8.GetBytes(path.Value)))}";
        return WorkspaceDocument.Create(key, path, bytes);
    }

    /// <summary>Binds preflighted, self-contained documents to a comparison workspace.</summary>
    /// <param name="name">The resolved application identifier.</param>
    /// <param name="documents">The preflighted document snapshots.</param>
    /// <returns>The compilation or refusal diagnostics.</returns>
    internal static (SemanticCompilation? Model, IEnumerable<Diagnostic> Diagnostics) Bind(string name, IReadOnlyList<WorkspaceDocument> documents)
    {
        var identity = ApplicationIdentity.Create(name);
        var workspace = ScreenplayWorkspace.Create(identity, name, [.. documents], SemanticIdentityCatalog.Empty(identity));
        var compilation = ComparedModel.WithoutIdentities(workspace).Workspace.Compilation;
        return (compilation.Success ? compilation.Value : null, compilation.Diagnostics);
    }

    /// <summary>Enforces per-line limits and deducts the document's lines from the shared budget.</summary>
    /// <param name="source">The admitted source text.</param>
    /// <param name="remainingLines">The remaining shared line budget.</param>
    /// <returns>Whether the source stayed within all line limits.</returns>
    internal static bool WithinLineLimits(string source, ref int remainingLines)
    {
        using var reader = new StringReader(source);
        while (reader.ReadLine() is { } line)
        {
            if (--remainingLines < 0 || _utf8.GetByteCount(line) > MaxLineBytes || !ValidIndent(line.AsSpan().TrimStart('\uFEFF'))) return false;
        }

        return true;
    }

    static bool ValidIndent(ReadOnlySpan<char> line)
    {
        var indent = 0;
        foreach (var character in line)
        {
            if (character == '\t') return false;
            if (character != ' ') break;
            if (++indent > MaxIndent) return false;
        }

        return true;
    }
}
