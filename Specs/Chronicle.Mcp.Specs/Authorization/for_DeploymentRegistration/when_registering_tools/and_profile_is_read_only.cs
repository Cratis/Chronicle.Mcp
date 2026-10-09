// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_DeploymentRegistration.when_registering_tools;

public class and_profile_is_read_only : given.a_deployment
{
    void Because() => Register();
    [Fact] void should_register_all_read_only_tools() => _protocol.ToolCollection.Select(tool => tool.ProtocolTool.Name).ShouldContainOnly(_catalog.Tools.Where(tool => tool.IsReadOnly).Select(tool => tool.Name).ToArray());
    [Fact] void should_register_no_job_control_tool() => _protocol.ToolCollection.Where(tool => !_catalog.Find(tool.ProtocolTool.Name).IsReadOnly).ShouldBeEmpty();
    [Fact] void should_register_the_call_filter() => _protocol.Filters.Request.CallToolFilters.Count.ShouldEqual(1);
}
