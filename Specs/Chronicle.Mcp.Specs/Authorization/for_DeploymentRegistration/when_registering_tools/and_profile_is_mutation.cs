// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_DeploymentRegistration.when_registering_tools;

public class and_profile_is_mutation : given.a_deployment
{
    void Establish() => _profile = "Mutation";
    void Because() => Register();
    [Fact] void should_register_every_tool() => _protocol.ToolCollection.Select(tool => tool.ProtocolTool.Name).ShouldContainOnly(_catalog.Tools.Select(tool => tool.Name).ToArray());
    [Fact] void should_register_the_call_filter() => _protocol.Filters.Request.CallToolFilters.Count.ShouldEqual(1);
}
