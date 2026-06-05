// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Jobs;
using Cratis.Chronicle.Contracts.Primitives;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools.Jobs;

/// <summary>
/// Tools for working with jobs in Chronicle.
/// </summary>
[McpServerToolType]
public static class JobTools
{
    /// <summary>
    /// Lists all jobs in a namespace, optionally filtered by status.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="status">Optional filter by job status. Can be a comma-separated list of statuses.</param>
    /// <returns>A collection of job descriptors.</returns>
    [McpServerTool(Name = "list_jobs")]
    [Description("Lists all jobs in a namespace, optionally filtered by status. Returns a collection of job descriptors with status, progress, and metadata.")]
    public static async Task<IEnumerable<JobDescriptor>> ListJobs(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("Optional filter by job status. Can be a comma-separated list of statuses (e.g. 'Running,PreparingJob').")] string? status = null)
    {
        var request = new GetJobsRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace)
        };

        var jobs = await services.Jobs.GetJobs(request);

        var jobsList = jobs.ToList();
        var result = jobsList.Select(job => ToDescriptor(job)).ToList();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusStrings = status
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.Trim().ToUpperInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (statusStrings.Count > 0)
            {
                result = result.Where(j => statusStrings.Contains(j.Status)).ToList();
            }
        }

        return result;
    }

    /// <summary>
    /// Gets a specific job by ID.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <returns>Either a job descriptor with full details including status changes and progress, or a job error.</returns>
    [McpServerTool(Name = "get_job")]
    [Description("Gets a specific job by ID. Returns either a job descriptor with full details including status changes and progress, or a job error (NotFound, TypeIsNotAJobStateType, TypeIsNotAssociatedWithAJobType).")]
    public static async Task<JobResult> GetJob(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The job identifier.")] Guid jobId,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null)
    {
        var request = new GetJobRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace),
            JobId = jobId
        };

        var result = await services.Jobs.GetJob(request);

        var oneOfValue = result.Value as OneOf<Job, JobError>;
        return oneOfValue.Value switch
        {
            Job job => new JobResult(ToDescriptor(job), null),
            JobError jobError => new JobResult(null, jobError),
            _ => throw new InvalidOperationException($"Unknown job result type: {result.Value?.GetType().Name}")
        };
    }

    /// <summary>
    /// Gets the job steps for a specific job.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <param name="status">Optional filter by job step status. Can be a comma-separated list of statuses.</param>
    /// <returns>A collection of job step descriptors.</returns>
    [McpServerTool(Name = "get_job_steps")]
    [Description("Gets the job steps for a specific job. Returns a collection of job step descriptors with status, progress, and metadata. Optionally filtered by step status.")]
    public static async Task<IEnumerable<JobStepDescriptor>> GetJobSteps(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The job identifier.")] Guid jobId,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null,
        [Description("Optional filter by job step status. Can be a comma-separated list of statuses (e.g. 'Running,CompletedSuccessfully').")] string? status = null)
    {
        var request = new GetJobStepsRequest
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace),
            JobId = jobId
        };

        if (!string.IsNullOrWhiteSpace(status))
        {
            foreach (var statusStr in status
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Enum.TryParse<JobStepStatus>(statusStr, ignoreCase: true, out var parsed))
                {
                    request.Statuses = request.Statuses.Concat(new[] { parsed }).ToArray();
                }
            }
        }

        var jobSteps = await services.Jobs.GetJobSteps(request);
        return jobSteps.Select(ToStepDescriptor);
    }

    /// <summary>
    /// Stops a specific job.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <returns>Awaitable task.</returns>
    [McpServerTool(Name = "stop_job")]
    [Description("Stops a specific job. The job will transition to the Stopped status and can be resumed later.")]
    public static async Task StopJob(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The job identifier.")] Guid jobId,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null)
    {
        await services.Jobs.Stop(new StopJob
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace),
            JobId = jobId
        });
    }

    /// <summary>
    /// Resumes a specific stopped job.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <returns>Awaitable task.</returns>
    [McpServerTool(Name = "resume_job")]
    [Description("Resumes a specific stopped job. The job must be in the Stopped status.")]
    public static async Task ResumeJob(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The job identifier.")] Guid jobId,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null)
    {
        await services.Jobs.Resume(new ResumeJob
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace),
            JobId = jobId
        });
    }

    /// <summary>
    /// Deletes a specific job.
    /// </summary>
    /// <param name="services">The Chronicle services.</param>
    /// <param name="configuration">The connection configuration used to resolve defaults.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="eventStore">The event store. Defaults to the configured event store.</param>
    /// <param name="namespace">The namespace. Defaults to the configured namespace.</param>
    /// <returns>Awaitable task.</returns>
    [McpServerTool(Name = "delete_job")]
    [Description("Deletes a specific job. The job will transition to the Removing status and be eventually removed from the system.")]
    public static async Task DeleteJob(
        IServices services,
        ChronicleConnectionConfiguration configuration,
        [Description("The job identifier.")] Guid jobId,
        [Description("The event store. Defaults to the configured event store.")] string? eventStore = null,
        [Description("The namespace. Defaults to the configured namespace.")] string? @namespace = null)
    {
        await services.Jobs.Delete(new DeleteJob
        {
            EventStore = configuration.ResolveEventStore(eventStore),
            Namespace = configuration.ResolveNamespace(@namespace),
            JobId = jobId
        });
    }

    static JobDescriptor ToDescriptor(Job job)
    {
        var statusChanges = (job.StatusChanges ?? Enumerable.Empty<JobStatusChanged>()).Select(scs => new StatusChangeDescriptor(
            scs.Status.ToString(),
            scs.Occurred,
            scs.ExceptionMessages ?? [],
            scs.ExceptionStackTrace ?? string.Empty));

        return new JobDescriptor(
            job.Id,
            job.Details ?? string.Empty,
            job.Type ?? string.Empty,
            job.Status.ToString(),
            job.Created,
            statusChanges,
            new JobProgressDescriptor(
                job.Progress.TotalSteps,
                job.Progress.SuccessfulSteps,
                job.Progress.FailedSteps,
                job.Progress.StoppedSteps,
                job.Progress.IsCompleted,
                job.Progress.IsStopped,
                job.Progress.Message ?? string.Empty));
    }

    static JobStepDescriptor ToStepDescriptor(JobStep jobStep)
    {
        var statusChanges = (jobStep.StatusChanges ?? Enumerable.Empty<JobStepStatusChanged>()).Select(sc => new JobStepStatusChangeDescriptor(
            sc.Status.ToString(),
            sc.Occurred,
            sc.ExceptionMessages ?? Array.Empty<string>(),
            sc.ExceptionStackTrace ?? string.Empty));

        return new JobStepDescriptor(
            jobStep.Id,
            jobStep.Type ?? string.Empty,
            jobStep.Name ?? string.Empty,
            jobStep.Status.ToString(),
            statusChanges,
            jobStep.Progress is not null ? new JobStepProgressDescriptor(
                jobStep.Progress.Percentage,
                jobStep.Progress.Message ?? string.Empty) : null);
    }
}
