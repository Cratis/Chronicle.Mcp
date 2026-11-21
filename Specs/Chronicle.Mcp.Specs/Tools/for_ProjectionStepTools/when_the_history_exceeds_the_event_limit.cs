// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_the_history_exceeds_the_event_limit : given.a_projection_step
{
    void Establish() => _response.Data = [Snapshot(1, """{"n":1}"""), Snapshot(2, """{"n":2}"""), Snapshot(3, """{"n":3}""")];

    async Task Because() => _result = await Step(maxEvents: 2);

    [Fact] void should_refuse_the_history() => _result.Status.ShouldEqual(ProjectionStepStatus.RefusedOverBudget);
    [Fact] void should_not_truncate_to_a_partial_history() => _result.Steps.ShouldBeEmpty();
    [Fact] void should_report_the_total_events() => _result.TotalEvents.ShouldEqual(3);
    [Fact] void should_say_the_server_read_the_whole_history() => _result.Note.ShouldContain("whole history");
}
