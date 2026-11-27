// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Tools for working with observers (reactors, reducers, projections) in Chronicle.
/// </summary>
[McpServerToolType]
public static class ObserverTools
{
    const int QuarantinedRunningStateValue = 5;

    /// <summary>
    /// Reads an observer's reported state, positions, and unresolved failure evidence without starting recovery.
    /// </summary>
    /// <param name="services">The authenticated Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="observerId">The exact observer identifier.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="eventSequenceId">The expected event sequence. Defaults to event-log.</param>
    /// <param name="cancellationToken">Cancellation for the scoped reads.</param>
    /// <returns>The observer diagnosis with bounded evidence.</returns>
    [McpServerTool(Name = "diagnose_observer", ReadOnly = true, OpenWorld = false)]
    [Description("Inspects one observer's reported running state, sequence positions, and unresolved failed partitions. Does not measure progress or start recovery.")]
    public static Task<ObserverDiagnosis> DiagnoseObserver(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The exact observer identifier.")] string observerId,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("The expected event sequence. Defaults to event-log.")] string eventSequenceId = "event-log",
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(observerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventSequenceId);
        var reader = new ObserverDiagnosisReader(
            services,
            new AllObserversRequest { EventStore = configuration.ResolveEventStore(eventStore), Namespace = configuration.ResolveNamespace(@namespace) },
            observerId,
            eventSequenceId,
            cancellationToken);
        return reader.Read();
    }

    /// <summary>
    /// Lists observers in a namespace, optionally filtered by type.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="type">Optional observer type filter: reactor, reducer, projection, or all.</param>
    /// <returns>A concise descriptor for each observer.</returns>
    [McpServerTool(Name = "list_observers", ReadOnly = true, OpenWorld = false)]
    [Description("Lists observers (projections, reactors, reducers, client observers) in a namespace, with health and replay status. Optionally filter by type.")]
    public static async Task<IEnumerable<ObserverDescriptor>> ListObservers(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("Observer type filter: reactor, reducer, projection, or all (default).")] string type = "all")
    {
        var observers = await services.Observers.GetObservers(new AllObserversRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace)
        });

        return FilterByType(observers, type).Select(ToDescriptor);
    }

    /// <summary>
    /// Shows detailed information about a specific observer.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="observerId">The identifier of the observer to show.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="eventSequenceId">The event sequence to inspect. Defaults to event-log.</param>
    /// <returns>Detailed information about the observer.</returns>
    [McpServerTool(Name = "get_observer", ReadOnly = true, OpenWorld = false)]
    [Description("Shows detailed information about a specific observer including its type, running state, owner, handled sequence numbers and observed event types.")]
    public static async Task<ObserverDetails> GetObserver(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The identifier of the observer to show.")] string observerId,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("The event sequence to inspect. Defaults to event-log.")] string eventSequenceId = "event-log")
    {
        var info = await services.Observers.GetObserverInformation(new GetObserverInformationRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace),
            ObserverId = observerId,
            EventSequenceId = eventSequenceId
        });

        var eventTypes = (info.EventTypes ?? []).Select(et => $"{et.Id}+{et.Generation}").ToList();

        return new ObserverDetails(
            info.Id,
            info.EventSequenceId,
            info.Type.ToString(),
            info.Owner.ToString(),
            info.RunningState.ToString(),
            IsQuarantined(info),
            NullableSequenceNumber(info.NextEventSequenceNumber),
            NullableSequenceNumber(info.LastHandledEventSequenceNumber),
            info.IsSubscribed,
            eventTypes);
    }

    static IEnumerable<ObserverInformation> FilterByType(IEnumerable<ObserverInformation> observers, string type)
    {
        if (string.IsNullOrWhiteSpace(type) || string.Equals(type, "all", StringComparison.OrdinalIgnoreCase))
        {
            return observers;
        }

        return Enum.TryParse<ObserverType>(type, ignoreCase: true, out var parsed)
            ? observers.Where(o => o.Type == parsed)
            : observers;
    }

    static ObserverDescriptor ToDescriptor(ObserverInformation observer) => new(
        observer.Id,
        observer.Type.ToString(),
        observer.RunningState.ToString(),
        IsQuarantined(observer),
        NullableSequenceNumber(observer.NextEventSequenceNumber),
        NullableSequenceNumber(observer.LastHandledEventSequenceNumber),
        observer.IsSubscribed);

    static bool IsQuarantined(ObserverInformation observer) =>
        string.Equals(observer.RunningState.ToString(), "Quarantined", StringComparison.OrdinalIgnoreCase) ||
        (int)observer.RunningState == QuarantinedRunningStateValue;

    static ulong? NullableSequenceNumber(ulong sequenceNumber) =>
        sequenceNumber == ulong.MaxValue ? null : sequenceNumber;
}
