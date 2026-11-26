// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Screenplay;
using Cratis.Screenplay.Comparison;
using Cratis.Screenplay.Diagnostics;
using Cratis.Screenplay.Semantics;
using Cratis.Screenplay.Workspaces;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

internal static class ConformanceModelSources
{
    const string LogicalPath = "application.play";
    const int MaxSourceBytes = 64 * 1024;
    const int MaxLines = 4096;
    const int MaxLineBytes = 2048;
    const int MaxIndent = 128;
    static readonly UTF8Encoding _utf8 = new(false, true);

    internal static bool Admit(string? source)
    {
        if (string.IsNullOrWhiteSpace(source)) return false;
        if (source.Length > MaxSourceBytes || _utf8.GetByteCount(source) > MaxSourceBytes) return false;
        using var reader = new StringReader(source);
        var lines = 0;
        while (reader.ReadLine() is { } line)
        {
            if (++lines > MaxLines) return false;
            if (_utf8.GetByteCount(line) > MaxLineBytes || !ValidIndent(line.AsSpan().TrimStart('\uFEFF'))) return false;
        }

        return true;
    }

    internal static (SemanticCompilation? Model, IEnumerable<Diagnostic> Diagnostics) Compile(string source, string? applicationName, string fallback)
    {
        var syntax = new ScreenplayCompiler().Parse(source, LogicalPath);
        if (!syntax.Success) return (null, syntax.Diagnostics);
        if (syntax.Value!.FileImports.Any()) return (null, []);

        var name = applicationName ?? syntax.Value.Domain?.Name ?? fallback;
        if (!ConformanceScope.Identifier(name)) return (null, []);
        var identity = ApplicationIdentity.Create(name);
        var path = PortablePlayPath.Parse(LogicalPath);
        // Keep the same stable document identity as the CLI's ComparisonInput.
        var key = $"file-{Convert.ToHexStringLower(SHA256.HashData(_utf8.GetBytes(path.Value)))}";
        var document = WorkspaceDocument.Create(key, path, _utf8.GetBytes(source));
        var workspace = ScreenplayWorkspace.Create(identity, name, [document], SemanticIdentityCatalog.Empty(identity));
        var compilation = ComparedModel.WithoutIdentities(workspace).Workspace.Compilation;
        return (compilation.Success ? compilation.Value : null, compilation.Diagnostics);
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
