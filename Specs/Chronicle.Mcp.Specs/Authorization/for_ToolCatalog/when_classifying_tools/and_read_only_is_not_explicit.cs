// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Authorization.for_ToolCatalog.when_classifying_tools;

public class and_read_only_is_not_explicit : Specification
{
    ToolCatalog _catalog;
    void Because() => _catalog = new(typeof(and_read_only_is_not_explicit).Assembly);
    [Fact] void should_classify_the_tool_as_mutating() => _catalog.Find("unclassified_tool").IsReadOnly.ShouldBeFalse();
    [McpServerTool(Name = "unclassified_tool")] static void UnclassifiedTool() { }
}
