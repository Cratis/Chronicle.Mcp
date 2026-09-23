// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_mcp_server_tools;

public class when_classifying_open_world : given.all_tools
{
    IReadOnlyList<string> _notClosed;

    void Because() => _notClosed = _tools
        .Where(tool => !tool.DeclaresFalse(nameof(McpServerToolAttribute.OpenWorld)))
        .Select(tool => tool.Name)
        .ToList();

    [Fact] void should_discover_the_tools() => _tools.ShouldNotBeEmpty();
    [Fact] void should_confine_every_tool_to_the_configured_chronicle_server() => _notClosed.ShouldBeEmpty();
}
