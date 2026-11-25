// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_binding_an_allowlist;

public class and_an_entry_has_a_legacy_target_field : given.an_allowlist
{
    void Establish() => _authorization["Allowlist"][0]["Targets"] = new JsonArray("private-job");
    void Because() => _error = Catch.Exception(ReplacePolicy);
    [Fact] void should_reject_the_unknown_setting() => _error.ShouldNotBeNull();
    [Fact] void should_not_silently_drop_a_target_restriction() => Catch.Exception(() => _ = _monitor.CurrentValue).Message.ShouldEqual($"Invalid setting: {McpOptionsBinding.SectionPath}:Authorization:Allowlist:0:Targets");
}
