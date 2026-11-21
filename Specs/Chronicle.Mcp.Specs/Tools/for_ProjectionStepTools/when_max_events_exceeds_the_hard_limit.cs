// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_max_events_exceeds_the_hard_limit : given.a_projection_step
{
    void Establish() => _response.Data = [.. Enumerable.Range(1, ProjectionStepTools.HardMaxEvents + 1).Select(sequence => Snapshot((ulong)sequence, "{}"))];

    async Task Because() => _result = await Step(maxEvents: int.MaxValue);

    [Fact] void should_refuse_the_history() => _result.Status.ShouldEqual(ProjectionStepStatus.RefusedOverBudget);
    [Fact] void should_not_truncate_to_a_partial_history() => _result.Steps.ShouldBeEmpty();
}
