// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_starting_with_layered_authorization;

public class and_an_empty_json_revocations_array_has_overlay_children : given.a_layered_policy
{
    void Establish()
    {
        WithEmptyArray("Authorization:RevokedPrincipals");
        _overlay.Replace(new Dictionary<string, string?> { ["Authorization:RevokedPrincipals:0"] = Principal });
    }

    async Task Because() => _error = await Catch.Exception(Start);
    [Fact] void should_reject_the_ambiguous_configuration() => _error.ShouldBeOfExactType<InvalidMcpConfiguration>();
    [Fact] void should_name_only_the_setting_path() => _error.Message.ShouldEqual($"Invalid setting: {McpOptionsBinding.SectionPath}:Authorization:RevokedPrincipals");
}
