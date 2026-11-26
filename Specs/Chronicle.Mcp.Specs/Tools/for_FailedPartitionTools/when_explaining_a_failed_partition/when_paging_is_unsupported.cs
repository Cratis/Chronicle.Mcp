// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Sequences;
using Cratis.Chronicle.Mcp.Tools;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class when_paging_is_unsupported : given.a_failed_partition_query
{
    void Establish() => _sequences.Page(Arg.Any<PageRequest>(), Arg.Any<CallContext>())
        .ThrowsAsync(new RpcException(new(StatusCode.Unimplemented, "Synthetic unsupported")));

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);

    [Fact] void should_report_unsupported() => _result.Status.ShouldEqual("unsupported");
    [Fact] void should_not_supply_an_event() => _result.FailedEvent.ShouldBeNull();
    [Fact] void should_still_recheck_and_not_fall_back() => ShouldOnlyUseTheFourReads();
    [Fact] void should_not_attempt_recovery() => ShouldNotMutate();
}
