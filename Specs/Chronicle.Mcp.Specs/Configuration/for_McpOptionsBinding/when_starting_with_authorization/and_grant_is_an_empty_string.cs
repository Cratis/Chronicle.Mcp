// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_starting_with_authorization;

public class and_grant_is_an_empty_string : given.a_bound_policy
{
    void Establish() => WithChildlessEmpty("Authorization:Grants:0");
    async Task Because() => _error = await Catch.Exception(Start);
    [Fact] void should_fail_policy_validation() => _error.ShouldBeOfExactType<OptionsValidationException>();
    [Fact] void should_not_bind_a_principal_from_removed_children() => _monitor.CurrentValue.Authorization.Grants[0].Principal.ShouldEqual(string.Empty);
}
