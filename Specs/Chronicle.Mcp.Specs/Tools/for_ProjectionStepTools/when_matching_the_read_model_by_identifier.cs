// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_matching_the_read_model_by_identifier : given.a_projection_step
{
    void Establish() => _response.Data = [Snapshot(1, """{"name":"Ada"}""")];

    async Task Because() => _result = await Step(readModel: ReadModelIdentifier);

    [Fact] void should_be_ok() => _result.Status.ShouldEqual(ProjectionStepStatus.Ok);
    [Fact] void should_ask_for_the_read_model_identifier() => _request.ReadModel.ShouldEqual(ReadModelIdentifier);
}
