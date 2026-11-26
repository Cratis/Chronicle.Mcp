// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Connections;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.EventStores;
using Cratis.Chronicle.Contracts.EventTypes;
using Cratis.Chronicle.Contracts.Namespaces;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.Projections;
using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Contracts.Sequences;
using Cratis.Chronicle.Mcp.Authorization;
using Cratis.Chronicle.Mcp.Tools.Design.Conformance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using ProtoBuf.Grpc;
using EventType = Cratis.Chronicle.Contracts.Events.EventType;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class a_namespace_audit : Authorization.Specs.given.a_policy
{
    protected new const string Store = "Library";
    protected new const string Namespace = "Default";
    protected const string Sequence = "event-log";
    protected const string Operation = "audit_screenplay_conformance";
    protected const string Source = "domain Library\nmodule Library\n  feature Registration\n    slice StateChange Register\n      command Register\n        id Uuid identifier\n        name String\n        produces Registered\n          for id\n          name = name\n      event Registered\n        name String\n";
    protected static readonly Guid _correlationId = new(0x233e8fb4, 0x4505, 0x4235, 0x87, 0xc0, 0x3e, 0x96, 0x11, 0x6c, 0x92, 0xb9);
    protected IServices _reads;
    protected IEventSequences _sequences;
    protected IObservers _observers;
    protected IEventTypes _eventTypes;
    protected ulong _tail;
    protected List<EventMetadataResponse> _metadata;
    protected List<MetadataAtRequest> _pages;
    protected ServiceProvider _provider;
    protected CallToolResult _result;

    protected JsonElement Body => _result.StructuredContent!.Value.GetProperty("untrustedData").GetProperty("content");

    void Establish()
    {
        _arguments.Clear();
        Argument("eventStore", Store);
        Argument("namespace", Namespace);
        Argument("sequence", Sequence);
        Argument("fromSequence", 0);
        Argument("toSequence", 2);
        Argument("modelSource", Source);
        _tail = 2;
        _metadata = [Append(0, "Registered", "Register"), Append(1, "Legacy"), Append(2, "Registered")];
        _pages = [];
        _reads = Substitute.For<IServices>();
        SetUpTargets();
        SetUpRegistry();
        SetUpMetadata();
        var connection = Substitute.For<IChronicleConnection, IChronicleServicesAccessor>();
        ((IChronicleServicesAccessor)connection).Services.Returns(_reads);
        _provider = new ServiceCollection().AddSingleton(_configuration).AddSingleton(_clock).AddSingleton<ConformanceConnection>()
            .AddKeyedSingleton("screenplay-conformance", connection).BuildServiceProvider();
    }

    protected void Argument<T>(string name, T value) => _arguments[name] = JsonSerializer.SerializeToElement(value);

    protected async Task Audit(CancellationToken cancellationToken = default)
    {
        var tool = McpServerTool.Create(
            _catalog.Find(Operation)!.Method,
            request => ActivatorUtilities.CreateInstance<ScreenplayConformanceTools>(request.Services!),
            new() { Services = _provider });
        var request = new RequestContext<CallToolRequestParams>(
            Substitute.For<McpServer>(),
            new() { Method = RequestMethods.ToolsCall },
            new() { Name = Operation, Arguments = _arguments })
        {
            Services = _provider,
            MatchedPrimitive = tool
        };
        var filter = new AuthorizedToolCalls(_catalog, new(_policy, _configuration, _clock), _monitor, NullLogger<AuthorizedToolCalls>.Instance);
        _result = await filter.Filter(tool.InvokeAsync)(request, cancellationToken);
    }

    protected static QueryResult<T> Success<T>(T data) => new() { Data = data };

    protected static EventTypeDetailsResponse Event(string id, uint generation = 1) => new()
    {
        Type = new EventType { Id = id, Generation = generation },
        Schema = "private-schema-marker"
    };

    protected static EventMetadataResponse Append(ulong position, string type, string? command = null) => new()
    {
        SequenceNumber = position,
        EventTypeId = type,
        Occurred = Now.AddMinutes(position),
        EventSourceId = "private-source-id",
        EventSourceName = "private-source-name",
        EventSourceType = "private-source-type",
        EventStreamId = "private-stream-id",
        EventStreamType = "private-stream-type",
        CorrelationId = _correlationId,
        CausedBy = new() { Subject = "private-identity", UserName = "private-username" },
        InitiatorType = Contracts.Patterns.InitiatorType.User,
        Tags = ["private-tags"],
        Subject = "private-subject",
        Causation =
        [
            new() { Type = "private-causation", Properties = new Dictionary<string, string> { ["private-property"] = "private-value" } },
            .. command is null ? Array.Empty<Causation>() : [new Causation { Type = "Command", Properties = new Dictionary<string, string> { ["commandType"] = command } }]
        ]
    };

    void SetUpTargets()
    {
        var stores = Substitute.For<IEventStores>();
        var namespaces = Substitute.For<INamespaces>();
        _sequences = Substitute.For<IEventSequences>();
        _reads.EventStores.Returns(stores);
        _reads.Namespaces.Returns(namespaces);
        _reads.Sequences.Returns(_sequences);
        stores.AllEventStores(Arg.Any<CallContext>()).Returns(Success<IEnumerable<EventStoreNamesResponse>>([new() { Name = Store }]));
        namespaces.AllNamespaces(Arg.Is<AllNamespacesRequest>(request => request.EventStore == Store), Arg.Any<CallContext>())
            .Returns(Success<IEnumerable<NamespaceNamesResponse>>([new() { Name = Namespace }]));
        _sequences.AllEventSequences(Arg.Is<AllEventSequencesRequest>(request => request.EventStore == Store && request.Namespace == Namespace), Arg.Any<CallContext>())
            .Returns(Success<IEnumerable<EventSequenceNamesResponse>>([new() { Id = Sequence }]));
        _sequences.TailSequenceNumber(Arg.Is<TailSequenceNumberRequest>(request => request.EventStore == Store && request.Namespace == Namespace && request.EventSequenceId == Sequence), Arg.Any<CallContext>())
            .Returns(_ => Success(new EventSequenceTailResponse { SequenceNumber = _tail }));
    }

    void SetUpRegistry()
    {
        _eventTypes = Substitute.For<IEventTypes>();
        _observers = Substitute.For<IObservers>();
        _reads.EventTypes.Returns(_eventTypes);
        _reads.Observers.Returns(_observers);
        var projections = Substitute.For<IProjections>();
        _reads.Projections.Returns(projections);
        projections.GetAllDefinitions(Arg.Is<GetAllDefinitionsRequest>(request => request.EventStore == Store), Arg.Any<CallContext>())
            .Returns(Array.Empty<ProjectionDefinition>());
        _eventTypes.AllEventTypes(Arg.Is<AllEventTypesRequest>(request => request.EventStore == Store), Arg.Any<CallContext>())
            .Returns(Success<IEnumerable<EventTypeDetailsResponse>>([Event("Registered", 2), Event("RegisteredOnly")]));
        _observers.GetObservers(Arg.Is<AllObserversRequest>(request => request.EventStore == Store && request.Namespace == Namespace), Arg.Any<CallContext>())
            .Returns([new ObserverInformation { Id = "Elsewhere", Type = ObserverType.Reactor, Owner = ObserverOwner.Client, EventSequenceId = "other-log", EventTypes = [new EventType { Id = "Registered", Generation = 2 }] }]);
    }

    void SetUpMetadata() => _sequences.MetadataAt(Arg.Any<MetadataAtRequest>(), Arg.Any<CallContext>()).Returns(call =>
    {
        var request = call.Arg<MetadataAtRequest>();
        _pages.Add(request);
        return Success<IEnumerable<EventMetadataResponse>>(_metadata.Where(item => request.SequenceNumbers.Contains(item.SequenceNumber)).ToArray());
    });

    void Destroy() => _provider.Dispose();
}
