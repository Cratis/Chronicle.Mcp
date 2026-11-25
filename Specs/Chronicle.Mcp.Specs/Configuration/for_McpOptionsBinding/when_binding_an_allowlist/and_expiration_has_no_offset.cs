// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_binding_an_allowlist;

public class and_expiration_has_no_offset : given.an_allowlist
{
    void Establish() => _authorization["ExpiresAt"] = "2027-01-01T00:00:00";
    void Because() => _error = Catch.Exception(ReplacePolicy);
    [Fact] void should_reject_ambiguous_local_time() => _error.ShouldNotBeNull();
    [Fact] void should_keep_the_policy_invalid_on_later_reads() => Catch.Exception(() => _ = _monitor.CurrentValue).ShouldBeOfExactType<InvalidMcpConfiguration>();
}
