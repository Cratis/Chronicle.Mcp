// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_oversized_fields : given.a_failed_partition_query
{
    readonly string _store = new('s', 513);
    readonly string _observer = new('o', 513);

    void Establish()
    {
        _failure.ObserverId = _observer;
        _failure.Partition = new('p', 513);
        _observerInformation[0].Id = _observer;
        var attempt = _failure.Attempts.Single();
        attempt.Messages = [new('m', 2049), ..Enumerable.Repeat("message", 10)];
        attempt.StackTrace = new('s', 8193);
        var row = _page.Data.Events.Single();
        row.Context.EventType.Id = new('t', 513);
        row.Context.EventSourceId = new('n', 513);
        row.Content = $"{{\"name\":\"{new string('n', 4096)}\"}}";
    }

    async Task Because() => _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId, eventStore: _store);

    [Fact] void should_flag_and_cap_the_message_list() => (_result.Attempt!.Messages.Count, _result.Attempt.MessagesTruncated).ShouldEqual((10, true));
    [Fact] void should_flag_and_cap_the_message_text() => (_result.Attempt!.Messages[0].Length, _result.Attempt.MessageTextTruncated).ShouldEqual((2048, true));
    [Fact] void should_flag_and_cap_the_stack() => (_result.Attempt!.StackTrace!.Length, _result.Attempt.StackTraceTruncated).ShouldEqual((8192, true));
    [Fact] void should_flag_and_cap_display_identifiers() => (_result.EventStore.Length, _result.ObserverId!.Length, _result.Partition!.Length, _result.IdentifiersTruncated)
        .ShouldEqual((512, 512, 512, true));
    [Fact] void should_use_the_full_store_and_observer_in_requests() => (_pageRequests.Single().EventStore, _failureRequests[^1].ObserverId).ShouldEqual((_store, _observer));
    [Fact] void should_flag_and_cap_event_header_text() => (_result.FailedEvent!.EventTypeId.Length, _result.FailedEvent.EventSourceId.Length, _result.FailedEvent.HeadersTruncated)
        .ShouldEqual((512, 512, true));
    [Fact] void should_omit_oversized_json_whole_but_keep_headers() => (_result.FailedEvent!.ContentStatus, _result.FailedEvent.Content, _result.FailedEvent.SequenceNumber)
        .ShouldEqual(("budget_exceeded", null, 1000UL));
}
