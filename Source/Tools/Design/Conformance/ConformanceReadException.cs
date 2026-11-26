// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

internal sealed class ConformanceReadException(string code) : Exception(code)
{
    internal const string Failed = "conformance_read_failed";
    internal const string MissingTarget = "conformance_target_not_found";
    internal const string Limit = "conformance_read_limit";
}
