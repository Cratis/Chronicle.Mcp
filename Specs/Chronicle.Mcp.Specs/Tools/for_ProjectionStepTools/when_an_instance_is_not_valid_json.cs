// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_an_instance_is_not_valid_json : given.a_projection_step
{
    void Establish() => _response.Data = [Snapshot(1, """{"name":"Ada"}"""), Snapshot(2, "not json")];

    async Task Because() => _result = await Step();

    [Fact] void should_still_be_ok() => _result.Status.ShouldEqual(ProjectionStepStatus.Ok);
    [Fact] void should_have_no_instance() => _result.Steps[1].Instance.ShouldBeNull();
    [Fact] void should_not_claim_a_removal() => _result.Steps[1].Removed.ShouldBeFalse();
    [Fact] void should_not_claim_the_step_unchanged() => _result.Steps[1].Unchanged.ShouldBeFalse();
    [Fact] void should_explain_the_step() => _result.Steps[1].Note.ShouldNotBeNull();
}
