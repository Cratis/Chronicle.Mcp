// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_the_key_is_blank : given.a_projection_step
{
    async Task Because() => _result = await Step(readModelKey: " ");

    [Fact] void should_report_an_invalid_request() => _result.Status.ShouldEqual(ProjectionStepStatus.InvalidRequest);
    [Fact] void should_not_look_up_the_read_model() => _readModels.DidNotReceiveWithAnyArgs().GetDefinitions(default!, default);
    [Fact] void should_not_ask_for_snapshots() => _explorer.DidNotReceiveWithAnyArgs().AllSnapshotsForReadModel(default!, default);
}
