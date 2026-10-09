// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Chronicle.Mcp.Authorization;

internal static partial class AuthorizedToolCallsLogging
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Tool denied: {Code} for {Operation}")]
    internal static partial void ToolDenied(this ILogger<AuthorizedToolCalls> logger, string code, string operation);
}
