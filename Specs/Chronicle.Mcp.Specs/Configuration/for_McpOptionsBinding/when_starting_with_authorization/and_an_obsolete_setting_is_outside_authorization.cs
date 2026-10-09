// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_starting_with_authorization;

public class and_an_obsolete_setting_is_outside_authorization : given.a_bound_policy
{
    void Establish() => _root["ManagementPort"] = "8080";
    async Task Because() => _error = await Catch.Exception(Start);
    [Fact] void should_start() => _error.ShouldBeNull();
    [Fact] void should_keep_the_authorization_policy() => _monitor.CurrentValue.Authorization.Grants.ShouldNotBeEmpty();
}
