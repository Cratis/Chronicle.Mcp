// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_starting_with_authorization;

public class and_operations_are_scalar : given.a_bound_policy
{
    void Establish() => _root["Authorization:Grants:0:Operations"] = "stop_job";
    async Task Because() => _error = await Catch.Exception(Start);
    [Fact] void should_reject_the_configuration() => _error.ShouldBeOfExactType<InvalidMcpConfiguration>();
    [Fact] void should_expose_only_a_setting_path() => _error.Message.StartsWith($"Invalid setting: {McpOptionsBinding.SectionPath}:Authorization", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_not_expose_a_value() => _error.Message.Contains("operator-private", StringComparison.Ordinal).ShouldBeFalse();
}
