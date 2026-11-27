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
    /// Normalized suffixes. Shorter suffixes also cover clientsecret, sharedaccesskey, and setcookie.
    /// Authorization assignments deliberately fail closed, even for harmless values such as denied.
    /// </summary>
    static readonly string[] _credentialKeySuffixes =
    [
        "password", "passwd", "pwd", "secret", "token", "apikey", "accesskey", "secretkey", "accountkey", "privatekey",
        "sharedaccesssignature", "credential", "credentials", "authorization", "cookie"
    ];

    /// <summary>
    /// Gets whether any text screened by this instance was withheld.
    /// </summary>
    internal bool DiagnosticsRedacted { get; private set; }

    [GeneratedRegex("""(?<key>(?:[A-Za-z0-9_.-]+ +)?[A-Za-z0-9_.-]+)(?:\\?["'])?\s*[:=]""", RegexOptions.NonBacktracking)]
    private static partial Regex CandidateAssignment { get; }

    [GeneratedRegex(
        """\bbearer\s+[A-Za-z0-9._~+/=-]{16,}|://[^\s/?#@]*:[^\s/?#@]+@|-----BEGIN [A-Z0-9 ]*PRIVATE KEY(?: BLOCK)?-----""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking)]
    private static partial Regex StandaloneCredentialMarker { get; }

    /// <summary>
    /// Screens one complete diagnostic field before any clipping.
    /// </summary>
    /// <param name="text">The original diagnostic text.</param>
    /// <returns>The original text or a placeholder if a credential marker was found.</returns>
    [return: NotNullIfNotNull(nameof(text))]
    internal string? Apply(string? text)
    {
        if (text is null || !ContainsCredentialMarker(text)) return text;
        DiagnosticsRedacted = true;
        return "[withheld]";
    }

    static bool ContainsCredentialMarker(string text) =>
        StandaloneCredentialMarker.IsMatch(text) || CandidateAssignment.Matches(text).Any(HasCredentialKey);

    static bool HasCredentialKey(Match assignment)
    {
        var key = NormalizeKey(assignment.Groups["key"].Value);
        return _credentialKeySuffixes.Any(suffix => key.EndsWith(suffix, StringComparison.Ordinal));
    }

    static string NormalizeKey(string key) => new(key.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}
