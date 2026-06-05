// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

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
    [McpServerTool(Name = "list_failed_partitions")]
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
}
