// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_binding_an_allowlist;

public class and_an_environment_collection_key_is_not_an_index : given.an_allowlist
{
    void Establish() => _overlay.Replace(new Dictionary<string, string?> { ["Authorization:Allowlist:0:Operations:invalid"] = "resume_job" });
    void Because() => _error = Catch.Exception(_overlay.Publish);
    [Fact] void should_reject_nonindexed_collection_children() => _error.ShouldNotBeNull();
    [Fact] void should_report_only_the_list_path() => Catch.Exception(() => _ = _monitor.CurrentValue).Message.ShouldEqual($"Invalid setting: {McpOptionsBinding.SectionPath}:Authorization:Allowlist:0:Operations:invalid");
}
