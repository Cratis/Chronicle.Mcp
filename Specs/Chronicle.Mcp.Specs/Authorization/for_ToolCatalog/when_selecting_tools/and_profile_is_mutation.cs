// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;

namespace Cratis.Chronicle.Mcp.Authorization.for_ToolCatalog.when_selecting_tools;

public class and_profile_is_mutation : Specification
{
    CatalogTool[] _tools;
    void Because() => _tools = new ToolCatalog(typeof(ToolCatalog).Assembly).ForProfile(DeploymentProfile.Mutation).ToArray();
    [Fact] void should_register_the_job_control_tools() => _tools.Where(tool => !tool.IsReadOnly).Select(tool => tool.Name).ShouldContainOnly("stop_job", "resume_job", "delete_job");
    [Fact] void should_register_every_tool() => _tools.Length.ShouldEqual(new ToolCatalog(typeof(ToolCatalog).Assembly).Tools.Count());
}
