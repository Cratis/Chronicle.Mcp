// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;
using Cratis.Chronicle.Contracts.Jobs;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_an_allowlist_is_narrowed_on_reload : given.a_layered_filter
{
    void Establish()
    {
        _json["Authorization"] = new JsonObject
        {
            ["ExpiresAt"] = Now.AddHours(1).ToString("O"),
            ["Allowlist"] = new JsonArray(new JsonObject { ["EventStore"] = Store, ["Namespace"] = Namespace, ["Operations"] = new JsonArray("stop_job") })
        };
        _jsonProvider.Replace(_json.ToJsonString());
    }

    async Task Because()
    {
        _firstResult = await Call();
        _json["Authorization"]["Allowlist"][0]["Operations"] = new JsonArray("resume_job");
        _jsonProvider.Replace(_json.ToJsonString());
        _jsonProvider.Publish();
        _result = await Call();
    }

    [Fact] void should_allow_before_reload() => (_firstResult.IsError ?? false).ShouldBeFalse();
    [Fact] void should_not_invoke_after_reload() => _services.Jobs.Received(1).StopJob(Arg.Any<StopJobRequest>());
    [Fact] void should_return_a_sanitized_denial() => _result.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString().ShouldEqual(DenialCodes.NotAuthorized);
}
