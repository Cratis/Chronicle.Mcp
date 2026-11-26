// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_sparse_sequence_numbers : given.a_failed_partition_query
{
    void Establish()
    {
        _failure.IsQuarantined = true;
        _page.Data.Events = [EventAt(1000), EventAt(900), EventAt(4), EventAt(0)];
    }

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);

    [Fact] void should_explain_the_exact_failed_event() => _result.FailedEvent!.SequenceNumber.ShouldEqual(1000UL);
    [Fact] void should_succeed() => _result.Status.ShouldEqual("explained");
    [Fact] void should_include_quarantine_state() => _result.IsQuarantined.ShouldEqual(true);
    [Fact] void should_include_the_event_type() => _result.FailedEvent!.EventTypeId.ShouldEqual("OrderPlaced");
    [Fact] void should_include_the_generation() => _result.FailedEvent!.Generation.ShouldEqual(2U);
    [Fact] void should_include_the_source() => _result.FailedEvent!.EventSourceId.ShouldEqual("order");
    [Fact] void should_include_the_correlation_id() => _result.FailedEvent!.CorrelationId.ShouldEqual(CorrelationId);
    [Fact] void should_include_the_occurrence_time() => _result.FailedEvent!.Occurred.ShouldEqual(Occurred);
    [Fact] void should_return_oldest_first_sequence_history() => _result.Context.Events.Select(row => row.SequenceNumber).ShouldEqual([0UL, 4UL, 900UL]);
    [Fact] void should_not_mark_a_complete_window_truncated() => _result.Context.Truncated.ShouldBeFalse();
    [Fact] void should_report_the_default_requested_limit() => _result.Context.RequestedLimit.ShouldEqual(10);
    [Fact] void should_not_apply_a_limit_to_the_default() => _result.Context.LimitApplied.ShouldBeFalse();
    [Fact] void should_seek_backward_from_the_failure_inclusively() => (_pageRequests.Single().Backward, _pageRequests.Single().From, _pageRequests.Single().Until)
        .ShouldEqual((true, (ulong?)1000, (ulong?)0));
    [Fact] void should_request_the_exact_event_plus_ten() => _pageRequests.Single().Count.ShouldEqual(11);
    [Fact] void should_not_filter_by_source_or_type() => (_pageRequests.Single().EventSourceId, _pageRequests.Single().EventTypeIds).ShouldEqual((null, null));
    [Fact] void should_use_only_the_three_read_endpoints() => ShouldOnlyUseTheFourReads();
    [Fact] void should_not_replay_retry_clear_compensate_or_append() => ShouldNotMutate();

    [Fact] void should_use_the_configured_store_for_every_read() => _failureRequests.Select(request => request.EventStore)
        .Append(_observerRequests.Single().EventStore).Append(_pageRequests.Single().EventStore).ShouldEqual(Enumerable.Repeat("ConfiguredStore", 4));
    [Fact] void should_use_the_configured_namespace_for_every_read() => _failureRequests.Select(request => request.Namespace)
        .Append(_observerRequests.Single().Namespace).Append(_pageRequests.Single().Namespace).ShouldEqual(Enumerable.Repeat("ConfiguredTenant", 4));
    [Fact] void should_only_filter_by_observer_on_the_final_lookup() => _failureRequests.Select(request => request.ObserverId).ShouldEqual([null, "observer"]);
    [Fact] void should_use_the_bound_service_call_context() => _callContexts.ShouldEqual(Enumerable.Repeat(default(CallContext), 4));
}
