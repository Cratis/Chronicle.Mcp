// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_access_is_denied : given.an_observer_query
{
    readonly RpcException _denial = new(new(StatusCode.PermissionDenied, PrivateDetail));
    Exception _error;

    void Establish() => _failures.GetFailedPartitions(Arg.Any<GetFailedPartitionsRequest>(), Arg.Any<CallContext>()).ThrowsAsync(_denial);

    async Task Because()
    {
        ClearCalls();
        _error = await Catch.Exception(Diagnose);
    }

    [Fact] void should_propagate_denial_for_sanitizing() => _error.ShouldBeSame(_denial);
    [Fact] void should_not_continue_after_denial() => ShouldOnlyRead(1, 1, 0);
}
