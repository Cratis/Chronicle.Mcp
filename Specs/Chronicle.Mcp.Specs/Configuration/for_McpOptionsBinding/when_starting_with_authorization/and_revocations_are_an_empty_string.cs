// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_starting_with_authorization;

public class and_revocations_are_an_empty_string : given.a_bound_policy
{
    void Establish() => _root["Authorization:RevokedPrincipals"] = string.Empty;
    async Task Because() => _error = await Catch.Exception(Start);
    [Fact] void should_accept_the_empty_string() => _error.ShouldBeNull();
    [Fact] void should_bind_an_empty_revocation_list() => _monitor.CurrentValue.Authorization.RevokedPrincipals.ShouldBeEmpty();
}
