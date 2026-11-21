// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.ReadModels;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_the_read_model_is_a_reducer : given.a_projection_step
{
    void Establish() => _definition.ObserverType = ReadModelObserverType.Reducer;

    async Task Because() => _result = await Step();

    [Fact] void should_report_an_unsupported_read_model() => _result.Status.ShouldEqual(ProjectionStepStatus.UnsupportedReadModel);
    [Fact] void should_not_ask_for_snapshots() => _explorer.DidNotReceiveWithAnyArgs().AllSnapshotsForReadModel(default!, default);
}
