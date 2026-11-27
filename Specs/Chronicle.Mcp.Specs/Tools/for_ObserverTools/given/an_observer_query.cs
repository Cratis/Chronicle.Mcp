// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Jobs;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Contracts.Recommendations;
using Cratis.Chronicle.Contracts.Sequences;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Tools;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProtoBuf.Grpc;
using ObserverScope = (string Store, string Tenant, string ObserverId, string Sequence);

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.given;

public class an_observer_query : Specification
{
    protected const string Store = "SampleStore";
    protected const string Namespace = "SampleTenant";
    protected const string ObserverId = "sample-observer";
    protected const string Sequence = "event-log";
    protected const string PrivateDetail = "Synthetic upstream exception detail";
    protected static readonly DateTimeOffset Occurred = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    protected IServices _services;
    protected IObservers _observers;
    protected IFailedPartitions _failures;
    protected IEventSequences _sequences;
    protected IJobs _jobs;
    protected IRecommendations _recommendations;
    protected ObserverInformation _observer;
    protected ChronicleConnectionConfiguration _configuration;
    protected ObserverDiagnosis _result;

    void Establish()
    {
        _configuration = new(Options.Create(new Configuration.McpServerOptions
        {
            UseCliConfiguration = false,
            EventStore = Store,
            Namespace = Namespace
        }));
        _observer = ActiveObserver();
        SubstituteServices();
        _observers.GetObservers(Arg.Any<AllObserversRequest>(), Arg.Any<CallContext>()).Returns(_ => new[] { _observer });
        WithFailures();
        WithTail(0);
    }

    protected static ObserverInformation ActiveObserver() => new()
    {
        Id = ObserverId,
        EventSequenceId = Sequence,
        RunningState = ObserverRunningState.Active,
        NextEventSequenceNumber = 1,
        HandledEventCount = 42,
        RunningStateChangedAt = Occurred
    };

    protected static FailedPartition Failure(string partition) => new()
    {
        Id = Guid.NewGuid(),
        ObserverId = ObserverId,
        Partition = partition
    };

    protected void WithFailures(params FailedPartition[] failures) =>
        _failures.GetFailedPartitions(Arg.Any<GetFailedPartitionsRequest>(), Arg.Any<CallContext>()).Returns(failures.AsEnumerable());

    protected void WithTail(ulong position) =>
        _sequences.TailSequenceNumber(Arg.Any<TailSequenceNumberRequest>(), Arg.Any<CallContext>()).Returns(
            new QueryResult<EventSequenceTailResponse> { Data = new() { SequenceNumber = position } });

    protected Task<ObserverDiagnosis> Diagnose() => ObserverTools.DiagnoseObserver(_services, _configuration, ObserverId);

    protected Task<ObserverDiagnosis> Diagnose(ObserverScope scope) =>
        ObserverTools.DiagnoseObserver(_services, _configuration, scope.ObserverId, scope.Store, scope.Tenant, scope.Sequence);

    protected void ClearCalls()
    {
        _services.ClearReceivedCalls();
        _observers.ClearReceivedCalls();
        _failures.ClearReceivedCalls();
        _sequences.ClearReceivedCalls();
        _jobs.ClearReceivedCalls();
        _recommendations.ClearReceivedCalls();
    }

    protected void ShouldOnlyRead(int observers, int failures, int tails)
    {
        var allowedServices = Enumerable.Repeat("get_Observers", observers)
            .Concat(Enumerable.Repeat("get_FailedPartitions", failures))
            .Concat(Enumerable.Repeat("get_Sequences", tails)).Order();
        CallsTo(_services).Order().ShouldEqual(allowedServices);
        CallsTo(_observers).ShouldEqual(Enumerable.Repeat(nameof(IObservers.GetObservers), observers));
        CallsTo(_failures).ShouldEqual(Enumerable.Repeat(nameof(IFailedPartitions.GetFailedPartitions), failures));
        CallsTo(_sequences).ShouldEqual(Enumerable.Repeat(nameof(IEventSequences.TailSequenceNumber), tails));
        Assert.Empty(CallsTo(_jobs));
        Assert.Empty(CallsTo(_recommendations));
    }

    protected void ShouldReadScope() => ShouldReadScope((Store, Namespace, ObserverId, Sequence));

    protected void ShouldReadScope(ObserverScope scope)
    {
        _observers.Received(2).GetObservers(Arg.Is<AllObserversRequest>(request => request.EventStore == scope.Store && request.Namespace == scope.Tenant), Arg.Any<CallContext>());
        _failures.Received(1).GetFailedPartitions(Arg.Is<GetFailedPartitionsRequest>(request => request.EventStore == scope.Store && request.Namespace == scope.Tenant && request.ObserverId == scope.ObserverId), Arg.Any<CallContext>());
        _sequences.Received(1).TailSequenceNumber(Arg.Is<TailSequenceNumberRequest>(request => IsUnfilteredScope(request, scope)), Arg.Any<CallContext>());
    }

    static bool IsUnfilteredScope(TailSequenceNumberRequest request, ObserverScope scope) =>
        request.EventStore == scope.Store && request.Namespace == scope.Tenant && request.EventSequenceId == scope.Sequence &&
        string.IsNullOrEmpty(request.EventTypeIds) && string.IsNullOrEmpty(request.EventSourceId) &&
        string.IsNullOrEmpty(request.EventSourceType) && string.IsNullOrEmpty(request.EventStreamId) && string.IsNullOrEmpty(request.EventStreamType);

    static IEnumerable<string> CallsTo(object substitute) => substitute.ReceivedCalls().Select(call => call.GetMethodInfo().Name);

    void SubstituteServices()
    {
        _services = Substitute.For<IServices>();
        _observers = Substitute.For<IObservers>();
        _failures = Substitute.For<IFailedPartitions>();
        _sequences = Substitute.For<IEventSequences>();
        _jobs = Substitute.For<IJobs>();
        _recommendations = Substitute.For<IRecommendations>();
        _services.Observers.Returns(_observers);
        _services.FailedPartitions.Returns(_failures);
        _services.Sequences.Returns(_sequences);
        _services.Jobs.Returns(_jobs);
        _services.Recommendations.Returns(_recommendations);
    }
}
