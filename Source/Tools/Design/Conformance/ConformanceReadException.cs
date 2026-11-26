// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Aborts an audit without disclosing upstream exception details.
/// </summary>
/// <param name="code">The stable caller-facing error code.</param>
internal sealed class ConformanceReadException(string code) : Exception(code)
{
    /// <summary>
    /// A Chronicle read failed or returned inconsistent data.
    /// </summary>
    internal const string Failed = "conformance_read_failed";

    /// <summary>
    /// The exact target does not exist.
    /// </summary>
    internal const string MissingTarget = "conformance_target_not_found";

    /// <summary>
    /// Chronicle evidence exceeded an audit read limit.
    /// </summary>
    internal const string Limit = "conformance_read_limit";
}
