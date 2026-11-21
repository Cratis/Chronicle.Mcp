// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_an_event_leaves_the_state_unchanged : given.a_projection_step
{
    void Establish() => _response.Data = [Snapshot(1, """{"name":"Ada"}"""), Snapshot(2, """{"name":"Ada"}""")];

    async Task Because() => _result = await Step();

    [Fact] void should_mark_the_step_unchanged() => _result.Steps[1].Unchanged.ShouldBeTrue();
    [Fact] void should_have_no_changed_properties() => _result.Steps[1].ChangedProperties.ShouldBeEmpty();
    [Fact] void should_not_mark_the_step_removed() => _result.Steps[1].Removed.ShouldBeFalse();
}
