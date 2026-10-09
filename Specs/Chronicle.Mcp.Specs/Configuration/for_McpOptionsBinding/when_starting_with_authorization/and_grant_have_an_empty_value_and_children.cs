// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_starting_with_authorization;

public class and_grant_have_an_empty_value_and_children : given.a_bound_policy
{
    void Establish()
    {
        _root["Authorization:Grants:0"] = string.Empty;
    }

    async Task Because() => _error = await Catch.Exception(Start);
    [Fact] void should_reject_the_ambiguous_configuration() => _error.ShouldBeOfExactType<InvalidMcpConfiguration>();
    [Fact] void should_name_only_the_setting_path() => _error.Message.ShouldEqual($"Invalid setting: {McpOptionsBinding.SectionPath}:Authorization:Grants:0");
}
