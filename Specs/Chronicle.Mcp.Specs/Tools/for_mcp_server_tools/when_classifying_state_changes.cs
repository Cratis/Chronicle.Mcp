// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_mcp_server_tools;

public class when_classifying_state_changes : given.all_tools
{
    IReadOnlyList<given.DeclaredTool> _stateChanging;

    void Because() => _stateChanging = _tools
        .Where(tool => !tool.DeclaresTrue(nameof(McpServerToolAttribute.ReadOnly)))
        .ToList();

    [Fact] void should_change_state_only_through_the_job_control_tools() => _stateChanging.Select(tool => tool.Name).ShouldContainOnly("stop_job", "resume_job", "delete_job");
    [Fact] void should_declare_destructive_explicitly_on_every_state_changing_tool() => _stateChanging.Where(tool => !tool.Declares(nameof(McpServerToolAttribute.Destructive))).Select(tool => tool.Name).ShouldBeEmpty();
    [Fact] void should_declare_idempotent_explicitly_on_every_state_changing_tool() => _stateChanging.Where(tool => !tool.Declares(nameof(McpServerToolAttribute.Idempotent))).Select(tool => tool.Name).ShouldBeEmpty();
    [Fact] void should_only_declare_deleting_a_job_as_destructive() => _stateChanging.Where(tool => tool.DeclaresTrue(nameof(McpServerToolAttribute.Destructive))).Select(tool => tool.Name).ShouldContainOnly("delete_job");
}
