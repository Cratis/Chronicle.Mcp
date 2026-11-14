// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace Cratis.Chronicle.Mcp.Authorization.for_UntrustedDataEnvelope.given;

public class a_result : Specification
{
    protected CallToolResult _original;
    protected CallToolResult _result;
    protected const string Tool = "get_events";

    void Establish() => _original = new()
    {
        StructuredContent = JsonSerializer.SerializeToElement(new { message = "stored event" }),
        Content = [new TextContentBlock { Text = "{\"message\":\"stored event\"}" }]
    };

    protected JsonElement Envelope => _result.StructuredContent!.Value.GetProperty("untrustedData");
    protected string Text => ((TextContentBlock)_result.Content[0]).Text;
    protected string Body => Text.Split('\n')[2];
    protected string Opening => Text.Split('\n')[1];
    protected string Closing => Text.Split('\n')[3];
}
