// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Mcp.Api;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp;

/// <summary>
/// Represents a set of tools for managing jobs in Chronicle.
/// </summary>
[McpServerToolType]
public static class JobTools
{
    /// <summary>
    /// Resumes a stopped job.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the job.</param>
    /// <param name="jobId">The unique identifier of the job to resume.</param>
    /// <returns>A confirmation message.</returns>
    [McpServerTool, Description("Resumes a stopped job. Jobs are background tasks like observer replays or event migrations. Use this when a job was previously stopped and needs to continue.")]
    public static async Task<string> ResumeJob(ChronicleApiClient chronicleApiClient, string eventStore, string @namespace, string jobId)
    {
        await chronicleApiClient.ResumeJob(eventStore, @namespace, jobId);

        return $"Job '{jobId}' resumed in event store '{eventStore}', namespace '{@namespace}'.";
    }

    /// <summary>
    /// Stops a running job.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the job.</param>
    /// <param name="jobId">The unique identifier of the job to stop.</param>
    /// <returns>A confirmation message.</returns>
    [McpServerTool, Description("Stops a running job. Jobs are background tasks like observer replays or event migrations. A stopped job can be resumed later with ResumeJob.")]
    public static async Task<string> StopJob(ChronicleApiClient chronicleApiClient, string eventStore, string @namespace, string jobId)
    {
        await chronicleApiClient.StopJob(eventStore, @namespace, jobId);

        return $"Job '{jobId}' stopped in event store '{eventStore}', namespace '{@namespace}'.";
    }

    /// <summary>
    /// Deletes a job permanently.
    /// </summary>
    /// <param name="chronicleApiClient">The <see cref="ChronicleApiClient"/> to use.</param>
    /// <param name="eventStore">The name of the event store.</param>
    /// <param name="namespace">The namespace of the job.</param>
    /// <param name="jobId">The unique identifier of the job to delete.</param>
    /// <returns>A confirmation message.</returns>
    [McpServerTool, Description("Deletes a job permanently. This removes the job and its history. Use with caution as this action cannot be undone.")]
    public static async Task<string> DeleteJob(ChronicleApiClient chronicleApiClient, string eventStore, string @namespace, string jobId)
    {
        await chronicleApiClient.DeleteJob(eventStore, @namespace, jobId);

        return $"Job '{jobId}' deleted in event store '{eventStore}', namespace '{@namespace}'.";
    }
}
