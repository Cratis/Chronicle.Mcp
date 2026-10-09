// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_starting_with_layered_authorization;

public class and_an_empty_json_revocations_array_has_no_overlay : given.a_layered_policy
{
    void Establish()
    {
        WithEmptyArray("Authorization:RevokedPrincipals");
    }

    async Task Because() => _error = await Catch.Exception(Start);
    [Fact] void should_accept_the_unambiguous_empty_array() => _error.ShouldBeNull();
    [Fact] void should_bind_an_empty_collection() => _monitor.CurrentValue.Authorization.RevokedPrincipals.ShouldBeEmpty();
}
