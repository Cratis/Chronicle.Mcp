// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Contracts.Sequences;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Tools;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.given;

public class a_failed_partition_query : Specification
{
    protected static readonly Guid FailureId = Guid.NewGuid();
    protected static readonly Guid CorrelationId = Guid.NewGuid();
    protected static readonly DateTimeOffset Occurred = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    protected readonly List<GetFailedPartitionsRequest> _failureRequests = [];
    protected readonly List<AllObserversRequest> _observerRequests = [];
    protected readonly List<PageRequest> _pageRequests = [];
    protected readonly List<CallContext> _callContexts = [];
    protected IServices _services;
    protected IFailedPartitions _failures;
    protected IObservers _observers;
    protected IEventSequences _sequences;
    protected ChronicleConnectionConfiguration _configuration;
    protected FailedPartition _failure;
    protected IEnumerable<FailedPartition>? _finalFailures;
    protected ObserverInformation[] _observerInformation;
    protected QueryResult<EventSequencePageResponse> _page;
    protected FailedPartitionExplanation _result;

    void Establish()
    {
        _configuration = new(Options.Create(new Configuration.McpServerOptions
        {
            UseCliConfiguration = false,
            EventStore = "ConfiguredStore",
            Namespace = "ConfiguredTenant"
        }));
        _failure = new() { Id = FailureId, ObserverId = "observer", Partition = "partition", Attempts = [AttemptAt(1000)] };
        _observerInformation = [new() { Id = "observer", EventSequenceId = "event-log" }];
        _page = new() { Data = new() { Events = [EventAt(1000)] } };
        SubstituteServices();
        CaptureRequests();
    }

    protected static FailedPartitionAttempt AttemptAt(ulong position) => new()
    {
        Occurred = Occurred,
        SequenceNumber = position,
        Kind = FailureKind.Handling,
        Messages = ["Sample failure"],
        StackTrace = "Sample stack"
    };

    protected static AppendedEventResponse EventAt(ulong position) => new()
    {
        Context = new()
        {
            SequenceNumber = position,
            EventType = new() { Id = "OrderPlaced", Generation = 2 },
            EventSourceId = "order",
            CorrelationId = CorrelationId,
            Occurred = Occurred
        },
        Content = "{\"item\":\"sample\"}"
    };

    protected void ShouldOnlyUseTheFourReads()
    {
        Assert.All(_services.ReceivedCalls(), call => Assert.Contains(call.GetMethodInfo().Name, new[] { "get_FailedPartitions", "get_Observers", "get_Sequences" }));
        _failures.ReceivedCalls().Select(call => call.GetMethodInfo().Name).ShouldEqual([nameof(IFailedPartitions.GetFailedPartitions), nameof(IFailedPartitions.GetFailedPartitions)]);
        _observers.ReceivedCalls().Select(call => call.GetMethodInfo().Name).ShouldEqual([nameof(IObservers.GetObservers)]);
        _sequences.ReceivedCalls().Select(call => call.GetMethodInfo().Name).ShouldEqual([nameof(IEventSequences.Page)]);
    }

    protected void ShouldNotMutate()
    {
        var methods = _observers.ReceivedCalls().Concat(_sequences.ReceivedCalls()).Select(call => call.GetMethodInfo().Name);
        string[] mutations = [nameof(IObservers.Replay), nameof(IObservers.ReplayPartition), nameof(IObservers.RetryPartition),
            nameof(IObservers.RetryFailedPartitions), nameof(IObservers.ClearFailedPartitions), nameof(IEventSequences.Compensate), nameof(IEventSequences.Append)];
        Assert.DoesNotContain(methods, mutations.Contains);
    }

    void SubstituteServices()
    {
        _services = Substitute.For<IServices>();
        _failures = Substitute.For<IFailedPartitions>();
        _observers = Substitute.For<IObservers>();
        _sequences = Substitute.For<IEventSequences>();
        _services.FailedPartitions.Returns(_failures);
        _services.Observers.Returns(_observers);
        _services.Sequences.Returns(_sequences);
        _services.ClearReceivedCalls();
    }

    void CaptureRequests()
    {
        _failures.GetFailedPartitions(Arg.Any<GetFailedPartitionsRequest>(), Arg.Any<CallContext>()).Returns(call =>
        {
            _failureRequests.Add(call.Arg<GetFailedPartitionsRequest>());
            _callContexts.Add(call.Arg<CallContext>());
            return _failureRequests.Count == 1 ? [_failure] : _finalFailures ?? [_failure];
        });
        _observers.GetObservers(Arg.Any<AllObserversRequest>(), Arg.Any<CallContext>()).Returns(call =>
        {
            _observerRequests.Add(call.Arg<AllObserversRequest>());
            _callContexts.Add(call.Arg<CallContext>());
            return _observerInformation;
        });
        _sequences.Page(Arg.Any<PageRequest>(), Arg.Any<CallContext>()).Returns(call =>
        {
            _pageRequests.Add(call.Arg<PageRequest>());
            _callContexts.Add(call.Arg<CallContext>());
            return _page;
        });
    }
}
