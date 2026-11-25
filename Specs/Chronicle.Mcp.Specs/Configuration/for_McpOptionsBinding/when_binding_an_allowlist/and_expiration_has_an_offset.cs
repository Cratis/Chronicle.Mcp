// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_binding_an_allowlist;

public class and_expiration_has_an_offset : given.an_allowlist
{
    void Establish() => _authorization["ExpiresAt"] = "2026-10-01T15:00:00+02:00";
    void Because() => ReplacePolicy();
    [Fact] void should_bind_the_same_instant() => _monitor.CurrentValue.Authorization.ExpiresAt.ShouldEqual(Now.AddHours(1));
}
