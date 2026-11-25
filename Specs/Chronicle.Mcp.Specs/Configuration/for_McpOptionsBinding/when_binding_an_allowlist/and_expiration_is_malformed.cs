// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_binding_an_allowlist;

public class and_expiration_is_malformed : given.an_allowlist
{
    void Establish() => _authorization["ExpiresAt"] = "private-invalid-timestamp";
    void Because() => _error = Catch.Exception(ReplacePolicy);
    [Fact] void should_reject_the_reload() => _error.ShouldNotBeNull();
    [Fact] void should_report_only_the_path() => Catch.Exception(() => _ = _monitor.CurrentValue).Message.ShouldEqual($"Invalid setting: {McpOptionsBinding.SectionPath}:Authorization:ExpiresAt");
}
