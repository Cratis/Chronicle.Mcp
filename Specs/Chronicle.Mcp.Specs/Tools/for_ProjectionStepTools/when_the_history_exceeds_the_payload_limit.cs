// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_the_history_exceeds_the_payload_limit : given.a_projection_step
{
    void Establish() => _response.Data = [Snapshot(1, $$"""{"text":"{{new string('x', 300 * 1024)}}"}""")];

    async Task Because() => _result = await Step();

    [Fact] void should_refuse_the_history() => _result.Status.ShouldEqual(ProjectionStepStatus.RefusedOverBudget);
    [Fact] void should_not_return_the_oversized_instance() => _result.Steps.ShouldBeEmpty();
}
