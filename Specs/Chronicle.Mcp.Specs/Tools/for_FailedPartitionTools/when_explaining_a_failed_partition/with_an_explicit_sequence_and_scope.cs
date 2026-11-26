// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_an_explicit_sequence_and_scope : given.a_failed_partition_query
{
    void Establish() => _observerInformation[0].EventSequenceId = "orders";

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId, "OrderStore", "OrderTenant", "orders");

    [Fact] void should_read_the_selected_sequence() => _pageRequests.Single().EventSequenceId.ShouldEqual("orders");
    [Fact] void should_explain_the_failure() => _result.Status.ShouldEqual("explained");
    [Fact] void should_honor_the_store_override_for_all_reads() => _failureRequests.Select(request => request.EventStore)
        .Append(_observerRequests.Single().EventStore).Append(_pageRequests.Single().EventStore).ShouldEqual(Enumerable.Repeat("OrderStore", 4));
    [Fact] void should_honor_the_namespace_override_for_all_reads() => _failureRequests.Select(request => request.Namespace)
        .Append(_observerRequests.Single().Namespace).Append(_pageRequests.Single().Namespace).ShouldEqual(Enumerable.Repeat("OrderTenant", 4));
}
