// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Withholds credential-like diagnostic text and tracks whether any field was withheld.
/// </summary>
internal sealed partial class DiagnosticTextScreen
{
    /// <summary>
    /// Gets whether any text screened by this instance was withheld.
    /// </summary>
    internal bool DiagnosticsRedacted { get; private set; }

    [GeneratedRegex(
        """\b(?:authorization|[\p{L}\p{Nd}_.-]*(?:password|passwd|pwd|secret|token|api(?:[_-]| +)?key|accesskey|accountkey|sharedaccesskey|sharedaccesssignature|credentials?|privatekey))(?:\\?["'])?\s*[:=]""" +
        """|\b(?:bearer|basic)\s+|\b(?:set-)?cookie\s*:""" +
        """|://[^\s@]+:[^\s@]+@|-----BEGIN [A-Z0-9 ]*PRIVATE KEY(?: BLOCK)?-----""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking)]
    private static partial Regex CredentialMarker { get; }

    /// <summary>
    /// Screens one complete diagnostic field before any clipping.
    /// </summary>
    /// <param name="text">The original diagnostic text.</param>
    /// <returns>The original text or a placeholder if a credential marker was found.</returns>
    [return: NotNullIfNotNull(nameof(text))]
    internal string? Apply(string? text)
    {
        if (text is null || !CredentialMarker.IsMatch(text)) return text;
        DiagnosticsRedacted = true;
        return "[withheld]";
    }
}
