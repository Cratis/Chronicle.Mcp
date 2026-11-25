// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_binding_an_allowlist;

public class and_environment_overrides_an_operation : given.an_allowlist
{
    void Establish() => _overlay.Replace(new Dictionary<string, string?> { ["Authorization:Allowlist:0:Operations:0"] = "resume_job" });
    void Because() => _overlay.Publish();
    [Fact] void should_bind_the_effective_operation() => _monitor.CurrentValue.Authorization.Allowlist.Single().Operations.Single().ShouldEqual("resume_job");
}
