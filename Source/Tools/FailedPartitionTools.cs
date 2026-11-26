// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.Sequences;
using Cratis.Chronicle.Mcp.Configuration;
using Grpc.Core;
using ModelContextProtocol.Server;

using static Cratis.Chronicle.Mcp.Tools.FailedPartitionExplanationFormatting;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Tools for inspecting failed observer partitions in Chronicle.
/// </summary>
[McpServerToolType]
public static class FailedPartitionTools
{
    /// <summary>
    /// Lists observer partitions that have failed and are paused.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="observerId">Optional observer identifier to filter by.</param>
    /// <returns>A concise descriptor for each failed partition.</returns>
    [McpServerTool(Name = "list_failed_partitions", ReadOnly = true, OpenWorld = false)]
    [Description("Lists observer partitions that have failed and are paused, including the failure reason and the sequence number where the failure occurred. Use to discover what needs investigation or recovery.")]
    public static async Task<IEnumerable<FailedPartitionDescriptor>> ListFailedPartitions(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("Optional observer identifier to filter by.")] string? observerId = null)
    {
        var failedPartitions = await services.FailedPartitions.GetFailedPartitions(new GetFailedPartitionsRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace),
            ObserverId = observerId
        });

        return failedPartitions.Select(failedPartition =>
        {
            var lastAttempt = failedPartition.Attempts
                .OrderByDescending(attempt => (DateTimeOffset?)attempt.Occurred ?? DateTimeOffset.MinValue)
                .FirstOrDefault();

            return new FailedPartitionDescriptor(
                failedPartition.Id,
                failedPartition.ObserverId,
                failedPartition.Partition,
                failedPartition.Attempts.Count(),
                lastAttempt?.SequenceNumber,
                lastAttempt is null ? null : string.Join(" | ", lastAttempt.Messages ?? []),
                lastAttempt is null ? null : (DateTimeOffset?)lastAttempt.Occurred);
        });
    }

    /// <summary>
    /// Inspects a failed partition's latest attempt and a bounded window ending at the exact failed event position.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="failedPartitionId">The failed partition identifier from the scoped failure list.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="eventSequenceId">The observer's event sequence. Defaults to event-log.</param>
    /// <param name="precedingCount">Maximum number of preceding events. Clamped to 0–50; defaults to 10.</param>
    /// <returns>The failure explanation or the reason a full explanation is unavailable.</returns>
    [McpServerTool(Name = "explain_failed_partition", ReadOnly = true, OpenWorld = false)]
    [Description("Inspects a failed partition's latest attempt, exact failed event, and bounded preceding sequence history. Does not perform recovery.")]
    public static async Task<FailedPartitionExplanation> ExplainFailedPartition(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The failed partition identifier from list_failed_partitions.")] Guid failedPartitionId,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("The observer's event sequence. Must match the observer metadata; defaults to event-log.")] string eventSequenceId = "event-log",
        [Description("Maximum number of preceding events, clamped to 0–50. Defaults to 10.")] int precedingCount = 10)
    {
        var scope = new GetFailedPartitionsRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace)
        };
        var result = ForScope(failedPartitionId, scope, eventSequenceId, precedingCount);
        var failure = await FindFailure(services, scope, failedPartitionId);
        if (failure is null) return result;

        var attempt = LatestAttempt(failure);
        result = WithFailure(result, failure, attempt);
        if (failure.IsResolved) return result with { Status = "resolved" };
        if (attempt is null) return result with { Status = "no_attempt" };
        if (attempt.SequenceNumber >= ulong.MaxValue - 2) return result with { Status = "no_event" };

        var sequence = await ObservedSequence(services, scope, failure.ObserverId);
        if (sequence is null) return result with { Status = "sequence_unavailable" };

        result = WithObservedSequence(result, sequence);
        if (!string.Equals(sequence, eventSequenceId, StringComparison.Ordinal)) return result with { Status = "sequence_mismatch" };

        var request = PageAt(scope, eventSequenceId, attempt.SequenceNumber, result.Context.Limit + 1);
        var page = await ReadPage(services.Sequences, request);
        var current = await RecheckFailure(services, scope, failure);
        if (current is null) return result with { Status = "disappeared" };
        if (DescribeChange(result, failure, attempt, current) is { } changed) return changed;

        result = result with { IsQuarantined = current.IsQuarantined };
        return page is null ? result with { Status = "unsupported" } : WithPage(result, page, attempt.SequenceNumber);
    }

    static async Task<FailedPartition?> FindFailure(IServices services, GetFailedPartitionsRequest scope, Guid id) =>
        (await services.FailedPartitions.GetFailedPartitions(scope)).SingleOrDefault(failure => failure.Id == id);

    static async Task<string?> ObservedSequence(IServices services, GetFailedPartitionsRequest scope, string observerId)
    {
        var observers = await services.Observers.GetObservers(new AllObserversRequest
        {
            EventStore = scope.EventStore,
            Namespace = scope.Namespace
        });
        var matches = observers.Where(observer => string.Equals(observer.Id, observerId, StringComparison.Ordinal)).Take(2).ToArray();
        return matches.Length == 1 && !string.IsNullOrWhiteSpace(matches[0].EventSequenceId) ? matches[0].EventSequenceId : null;
    }

    static PageRequest PageAt(GetFailedPartitionsRequest scope, string sequenceId, ulong position, int count) => new()
    {
        EventStore = scope.EventStore,
        Namespace = scope.Namespace,
        EventSequenceId = sequenceId,
        From = position,
        Until = 0,
        Count = count,
        Backward = true
    };

    static async Task<EventSequencePageResponse?> ReadPage(IEventSequences sequences, PageRequest request)
    {
        try
        {
            return QueryResults.Unwrap(await sequences.Page(request));
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.Unimplemented)
        {
            return null;
        }
    }

    static Task<FailedPartition?> RecheckFailure(IServices services, GetFailedPartitionsRequest scope, FailedPartition failure)
    {
        var request = new GetFailedPartitionsRequest
        {
            EventStore = scope.EventStore,
            Namespace = scope.Namespace,
            ObserverId = failure.ObserverId
        };
        return FindFailure(services, request, failure.Id);
    }

    static FailedPartitionExplanation? DescribeChange(
        FailedPartitionExplanation result,
        FailedPartition initial,
        FailedPartitionAttempt attempt,
        FailedPartition current)
    {
        var latest = LatestAttempt(current);
        if (current.IsResolved) return WithFailure(result, current, latest) with { Status = "resolved" };
        return SamePartition(initial, current) && IsSameAttempt(attempt, latest) ? null : result with { Status = "changed" };
    }

    static bool SamePartition(FailedPartition initial, FailedPartition current) =>
        initial.ObserverId == current.ObserverId && initial.Partition == current.Partition;

    static bool IsSameAttempt(FailedPartitionAttempt attempt, FailedPartitionAttempt? latest) =>
        latest is not null &&
        attempt.SequenceNumber == latest.SequenceNumber &&
        (DateTimeOffset?)attempt.Occurred == (DateTimeOffset?)latest.Occurred &&
        attempt.Kind == latest.Kind &&
        attempt.StackTrace == latest.StackTrace &&
        (attempt.Messages ?? []).SequenceEqual(latest.Messages ?? [], StringComparer.Ordinal);
}
