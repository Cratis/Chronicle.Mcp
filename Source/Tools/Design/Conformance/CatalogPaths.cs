// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Cratis.Screenplay.Files;
using Cratis.Screenplay.Workspaces;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Validates catalog names, operator roots, portable document paths, and authored import patterns.
/// </summary>
internal static class CatalogPaths
{
    const int MaxRelativeLength = 240;
    const int MaxRelativeDepth = 16;
    static readonly UTF8Encoding _utf8 = new(false, true);

    /// <summary>Checks the restricted ASCII identifier used for model names and root aliases.</summary>
    /// <param name="name">The catalog identifier to check.</param>
    /// <returns>Whether the identifier is admitted.</returns>
    internal static bool IsName([NotNullWhen(true)] string? name) =>
        name is { Length: > 0 and <= 64 } && name.All(character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-');

    /// <summary>Checks the absolute Linux path of a configured root, excluding the filesystem root.</summary>
    /// <param name="path">The configured path to check.</param>
    /// <returns>Whether the root path is admitted.</returns>
    internal static bool IsRoot(string? path)
    {
        if (path is not { Length: > 1 and <= 4095 } || path[0] != '/' || path.Any(char.IsControl) || !path[1..].Split('/').All(IsComponent)) return false;
        try
        {
            return _utf8.GetByteCount(path) <= 4095;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }

    /// <summary>Admits only an already-normalized, portable relative .play path.</summary>
    /// <param name="path">The document path to check.</param>
    /// <param name="portable">The resulting path when admitted.</param>
    /// <returns>Whether the document path is admitted.</returns>
    internal static bool TryDocument(string? path, [NotNullWhen(true)] out PortablePlayPath? portable)
    {
        portable = null;
        if (!IsRelative(path) || !PortablePlayPath.TryParse(path, out var parsed) || parsed.Value != path) return false;
        portable = parsed;
        return true;
    }

    /// <summary>Checks a raw file import, including relative glob patterns.</summary>
    /// <param name="pattern">The authored import pattern.</param>
    /// <returns>Whether the pattern is admitted.</returns>
    internal static bool IsImport(string pattern) =>
        IsRelative(pattern) && (PlayGlob.HasWildcard(pattern) || TryDocument(pattern, out _));

    static bool IsRelative([NotNullWhen(true)] string? path)
    {
        if (path is not { Length: > 0 and <= MaxRelativeLength } || path.Any(IsForbidden)) return false;
        var components = path.Split('/');
        return components.Length <= MaxRelativeDepth && components.All(IsComponent);
    }

    static bool IsForbidden(char value) => char.IsControl(value) || value is '\\' or ':' or '"' or '|' or '<' or '>';

    static bool IsComponent(string component) => component.Length > 0 && component is not ("." or "..") && component[^1] is not ('.' or ' ');
}
