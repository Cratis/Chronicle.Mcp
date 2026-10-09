// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Chronicle.Mcp.Authorization.for_ToolCatalog;

public class when_inspecting_mutating_tools : Specification
{
    CatalogTool[] _tools;
    void Because() => _tools = new ToolCatalog(typeof(ToolCatalog).Assembly).Tools.Where(tool => !tool.IsReadOnly).ToArray();
    [Fact] void should_inspect_all_three_job_mutations() => _tools.Length.ShouldEqual(3);
    [Fact] void should_have_exactly_one_target_on_every_mutation() => _tools.Where(tool => tool.Method.GetParameters().Count(parameter => parameter.IsDefined(typeof(AuthorizationTargetAttribute))) != 1).ShouldBeEmpty();
    [Fact] void should_have_an_event_store_on_every_mutation() => _tools.Where(tool => !tool.Method.GetParameters().Any(parameter => parameter.Name == "eventStore")).ShouldBeEmpty();
    [Fact] void should_have_a_namespace_on_every_mutation() => _tools.Where(tool => !tool.Method.GetParameters().Any(parameter => parameter.Name == "namespace")).ShouldBeEmpty();
}
