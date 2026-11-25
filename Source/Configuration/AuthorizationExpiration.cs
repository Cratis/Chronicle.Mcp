// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text.RegularExpressions;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Validates policy timestamps without interpreting a missing offset as machine-local time.
/// </summary>
internal static partial class AuthorizationExpiration
{
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,7})?(?:Z|[+-]\d{2}:\d{2})$", RegexOptions.CultureInvariant, 1000)]
    private static partial Regex Pattern { get; }

    /// <summary>
    /// Checks for an ISO-8601 timestamp with an explicit offset or Z.
    /// </summary>
    /// <param name="value">The raw configured timestamp.</param>
    /// <returns>Whether the timestamp has an unambiguous valid instant.</returns>
    public static bool IsValid(string? value) => value is not null && Pattern.IsMatch(value) &&
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
}
