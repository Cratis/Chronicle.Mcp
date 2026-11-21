// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_an_event_removes_the_instance : given.a_projection_step
{
    void Establish() => _response.Data = [Snapshot(1, """{"name":"Ada"}"""), Snapshot(2, "{}")];

    async Task Because() => _result = await Step();

    [Fact] void should_mark_the_step_removed() => _result.Steps[1].Removed.ShouldBeTrue();
    [Fact] void should_not_mark_the_step_unchanged() => _result.Steps[1].Unchanged.ShouldBeFalse();
    [Fact] void should_list_the_removed_properties() => _result.Steps[1].ChangedProperties.ShouldContainOnly("name");
    [Fact] void should_not_mark_the_first_step_removed() => _result.Steps[0].Removed.ShouldBeFalse();
}
