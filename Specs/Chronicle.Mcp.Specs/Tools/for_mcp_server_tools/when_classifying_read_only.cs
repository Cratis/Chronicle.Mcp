// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_mcp_server_tools;

public class when_classifying_read_only : given.all_tools
{
    IReadOnlyList<string> _unclassified;

    void Because() => _unclassified = _tools
        .Where(tool => !tool.Declares(nameof(McpServerToolAttribute.ReadOnly)))
        .Select(tool => tool.Name)
        .ToList();

    [Fact] void should_discover_the_tools() => _tools.ShouldNotBeEmpty();
    [Fact] void should_declare_read_only_explicitly_on_every_tool() => _unclassified.ShouldBeEmpty();
}
