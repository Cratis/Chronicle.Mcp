// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Contracts.ReadModelExplorer;
using Cratis.Chronicle.Contracts.ReadModels;
using Cratis.Chronicle.Contracts.Sequences;
using Cratis.Chronicle.Mcp.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProtoBuf.Grpc;

using ExplorerEvent = Cratis.Chronicle.Contracts.ReadModelExplorer.Event;

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools.given;

public class a_projection_step : Specification
{
    protected const string ReadModelName = "Accounts";
    protected const string ReadModelIdentifier = "accounts-read-model";
    protected const string Key = "account-1";

    protected IServices _services;
    protected IReadModels _readModels;
    protected IReadModelExplorer _explorer;
    protected ChronicleConnectionConfiguration _configuration;
    protected ReadModelDefinition _definition;
    protected AllSnapshotsForReadModelRequest _request;
    protected CallContext _callContext;
    protected QueryResult<IEnumerable<ReadModelSnapshotResponse>> _response;
    protected ProjectionStepResult _result;

    void Establish()
    {
        _services = Substitute.For<IServices>();
        _readModels = Substitute.For<IReadModels>();
        _explorer = Substitute.For<IReadModelExplorer>();
        _services.ReadModels.Returns(_readModels);
        _services.ReadModelExplorer.Returns(_explorer);
        _configuration = new(Options.Create(new McpServerOptions
        {
            UseCliConfiguration = false,
            EventStore = "ConfiguredStore",
            Namespace = "ConfiguredTenant"
        }));
        _definition = new()
        {
            Type = new ReadModelType { Identifier = ReadModelIdentifier, Generation = 1 },
            ContainerName = ReadModelName,
            ObserverType = ReadModelObserverType.Projection,
            Owner = ReadModelOwner.Server
        };
        _readModels.GetDefinitions(Arg.Any<GetDefinitionsRequest>(), Arg.Any<CallContext>())
            .Returns(_ => new GetDefinitionsResponse { ReadModels = [_definition] });
        _response = new() { Data = [] };
        _explorer.AllSnapshotsForReadModel(Arg.Any<AllSnapshotsForReadModelRequest>(), Arg.Any<CallContext>()).Returns(call =>
        {
            _request = call.Arg<AllSnapshotsForReadModelRequest>();
            _callContext = call.Arg<CallContext>();
            return _response;
        });
    }

    protected static ReadModelSnapshotResponse Snapshot(ulong sequenceNumber, string instance, string content = "{}") => new()
    {
        Occurred = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero).AddSeconds(sequenceNumber),
        CorrelationId = Guid.Parse($"00000000-0000-0000-0000-{sequenceNumber:D12}"),
        Instance = instance,
        Events =
        [
            new ExplorerEvent
            {
                Context = new EventContext
                {
                    SequenceNumber = sequenceNumber,
                    EventType = new EventType { Id = $"event-{sequenceNumber}", Generation = 1 },
                    EventSourceId = Key,
                    CorrelationId = Guid.Parse($"00000000-0000-0000-0000-{sequenceNumber:D12}")
                },
                Content = content
            }
        ]
    };

    protected Task<ProjectionStepResult> Step(string readModel = ReadModelName, string readModelKey = Key, int? maxEvents = null, CancellationToken cancellationToken = default) =>
        ProjectionStepTools.StepProjection(_services, _configuration, readModel, readModelKey, maxEvents: maxEvents, cancellationToken: cancellationToken);
}
