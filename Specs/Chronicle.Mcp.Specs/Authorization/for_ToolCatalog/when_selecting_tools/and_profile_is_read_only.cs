// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;

namespace Cratis.Chronicle.Mcp.Authorization.for_ToolCatalog.when_selecting_tools;

public class and_profile_is_read_only : Specification
{
    CatalogTool[] _tools;
    void Because() => _tools = new ToolCatalog(typeof(ToolCatalog).Assembly).ForProfile(DeploymentProfile.ReadOnly).ToArray();
    [Fact] void should_register_some_read_tools() => _tools.ShouldNotBeEmpty();
    [Fact] void should_register_no_mutating_tool() => _tools.Where(tool => !tool.IsReadOnly).ShouldBeEmpty();
}
