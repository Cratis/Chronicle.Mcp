// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_binding;

public class and_profile_is_unknown : Specification
{
    Exception? _error;
    void Because() => _error = Catch.Exception(() => McpOptionsBinding.Bind(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Profile"] = "unknown-sensitive-value" }).Build(), new()));
    [Fact] void should_refuse_binding() => _error.ShouldBeOfExactType<InvalidMcpConfiguration>();
    [Fact] void should_name_the_path_without_the_value() => _error.Message.ShouldEqual($"Invalid setting: {McpOptionsBinding.SectionPath}:Profile");
}
