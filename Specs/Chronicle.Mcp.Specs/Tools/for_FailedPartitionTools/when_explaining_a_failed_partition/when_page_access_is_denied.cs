// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Sequences;
using Cratis.Chronicle.Mcp.Tools;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class when_page_access_is_denied : given.a_failed_partition_query
{
    readonly RpcException _denial = new(new(StatusCode.PermissionDenied, "Synthetic denial"));
    Exception _error;

    void Establish() => _sequences.Page(Arg.Any<PageRequest>(), Arg.Any<CallContext>()).ThrowsAsync(_denial);

    async Task Because() => _error = await Catch.Exception(() => FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId));

    [Fact] void should_propagate_the_denial_for_sanitizing() => _error.ShouldBeSame(_denial);
}
