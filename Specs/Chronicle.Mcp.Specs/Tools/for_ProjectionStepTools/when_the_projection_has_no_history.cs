// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_the_projection_has_no_history : given.a_projection_step
{
    async Task Because() => _result = await Step();

    [Fact] void should_report_no_history() => _result.Status.ShouldEqual(ProjectionStepStatus.NoHistory);
    [Fact] void should_have_no_steps() => _result.Steps.ShouldBeEmpty();
    [Fact] void should_explain_the_status() => _result.Note.ShouldNotBeNull();
}
