// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_binding_an_allowlist;

public class and_empty_json_operations_have_environment_children : given.an_allowlist
{
    void Establish()
    {
        _authorization["Allowlist"][0]["Operations"] = new JsonArray();
        _overlay.Replace(new Dictionary<string, string?> { ["Authorization:Allowlist:0:Operations:0"] = "stop_job" });
    }

    void Because() => _error = Catch.Exception(ReplacePolicy);
    [Fact] void should_reject_ambiguous_merged_arrays() => _error.ShouldNotBeNull();
    [Fact] void should_report_only_the_path() => Catch.Exception(() => _ = _monitor.CurrentValue).Message.ShouldEqual($"Invalid setting: {McpOptionsBinding.SectionPath}:Authorization:Allowlist:0:Operations");
}
