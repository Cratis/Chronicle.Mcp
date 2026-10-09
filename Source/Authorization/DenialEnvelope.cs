// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Creates sanitized tool errors with identical structured and text content.
/// </summary>
public static class DenialEnvelope
{
    /// <summary>
    /// Creates an error containing only a denial code and operation name.
    /// </summary>
    /// <param name="code">The stable denial code.</param>
    /// <param name="operation">The catalog tool name.</param>
    /// <returns>The MCP error result.</returns>
    public static CallToolResult Create(string code, string operation)
    {
        var content = JsonSerializer.SerializeToElement(new { error = new { code, operation } });

        return new()
        {
            IsError = true,
            StructuredContent = content,
            Content = [new TextContentBlock { Text = content.GetRawText() }]
        };
    }
}
