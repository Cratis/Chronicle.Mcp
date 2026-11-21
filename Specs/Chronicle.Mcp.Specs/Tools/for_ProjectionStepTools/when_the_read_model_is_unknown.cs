// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_the_read_model_is_unknown : given.a_projection_step
{
    async Task Because() => _result = await Step(readModel: "Unknown");

    [Fact] void should_report_an_unknown_read_model() => _result.Status.ShouldEqual(ProjectionStepStatus.UnknownReadModel);
    [Fact] void should_not_ask_for_snapshots() => _explorer.DidNotReceiveWithAnyArgs().AllSnapshotsForReadModel(default!, default);
}
