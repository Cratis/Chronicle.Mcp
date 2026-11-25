// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.given;

public class an_allowlist : a_layered_policy
{
    protected JsonObject _authorization;

    void Establish()
    {
        _authorization = new()
        {
            ["ExpiresAt"] = Now.AddHours(1).ToString("O"),
            ["Allowlist"] = new JsonArray(new JsonObject { ["EventStore"] = Store, ["Namespace"] = Namespace, ["Operations"] = new JsonArray("stop_job") })
        };
        _json["Authorization"] = _authorization;
        ReplacePolicy();
    }

    protected void ReplacePolicy()
    {
        _jsonProvider.Replace(_json.ToJsonString());
        _jsonProvider.Publish();
    }
}
